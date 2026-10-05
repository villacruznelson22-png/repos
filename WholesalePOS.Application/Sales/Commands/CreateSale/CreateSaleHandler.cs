using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Sales.Commands.CreateSale;

public class CreateSaleHandler
    : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSaleHandler(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateSaleCommand request,
        CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------
        // Customer validation
        // ---------------------------------------------------------

        if (request.CustomerId.HasValue)
        {
            var customer =
                await _customerRepository.GetByIdAsync(
                    request.CustomerId.Value,
                    cancellationToken);

            if (customer is null)
            {
                throw new NotFoundException(
                    $"Customer '{request.CustomerId.Value}' was not found.");
            }

            if (!customer.IsActive)
            {
                throw new InvalidOperationException(
                    $"Customer '{request.CustomerId.Value}' is inactive and cannot be used for a sale.");
            }
        }

        // ---------------------------------------------------------
        // Product validation
        // ---------------------------------------------------------

        var productIds = request.Lines
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = new Dictionary<Guid, Product>();

        foreach (var productId in productIds)
        {
            var product =
                await _productRepository.GetByIdAsync(
                    productId,
                    cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    $"Product '{productId}' was not found.");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product '{productId}' is inactive and cannot be sold.");
            }

            products.Add(product.Id, product);
        }

        // ---------------------------------------------------------
        // Create sale
        // ---------------------------------------------------------

        var sale = new Sale(
            request.CustomerId,
            request.OccurredAt,
            request.ReferenceNumber,
            request.Notes);

        // ---------------------------------------------------------
        // Create sale lines
        // ---------------------------------------------------------
        //
        // Important:
        // UnitCost is intentionally NOT captured here.
        //
        // The sale is still a Draft, so the current inventory
        // cost may change before checkout.
        //
        // The historical UnitCost will be captured later during
        // CheckoutSale from the current InventoryBalance MWAC.
        // ---------------------------------------------------------

        foreach (var requestLine in request.Lines)
        {
            var line = new SaleLine(
                sale.Id,
                requestLine.ProductId,
                requestLine.Quantity,
                new Money(requestLine.UnitSellingPrice));

            sale.AddLine(line);
        }

        // ---------------------------------------------------------
        // Persist
        // ---------------------------------------------------------

        await _saleRepository.AddAsync(
            sale,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return sale.Id;
    }
}