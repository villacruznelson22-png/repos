using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Sales.Commands.UpdateSale;

public sealed class UpdateSaleHandler
    : IRequestHandler<UpdateSaleCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSaleHandler(
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

    public async Task Handle(
        UpdateSaleCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(
                request.CustomerId.Value,
                cancellationToken);

            if (customer is null)
                throw CustomerErrors.NotFound(request.CustomerId.Value);

            if (!customer.IsActive)
                throw new InvalidOperationException(
                    $"Customer '{request.CustomerId.Value}' is inactive and cannot be used for a sale.");
        }

        var requestedProductIds = request.Lines
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = new Dictionary<Guid, Product>();

        foreach (var productId in requestedProductIds)
        {
            var product = await _productRepository.GetByIdAsync(
                productId,
                cancellationToken);

            if (product is null)
                throw ProductErrors.NotFound(productId);

            if (!product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{productId}' is inactive and cannot be sold.");

            products.Add(productId, product);
        }

        sale.ChangeOccurredAt(request.OccurredAt);
        sale.ChangeCustomer(request.CustomerId);
        sale.ChangeReferenceNumber(request.ReferenceNumber);
        sale.ChangeNotes(request.Notes);

        var requestedByProduct = request.Lines
            .ToDictionary(x => x.ProductId);

        foreach (var existingLine in sale.Lines.ToList())
        {
            if (!requestedByProduct.ContainsKey(existingLine.ProductId))
            {
                sale.RemoveLine(existingLine.Id);
                continue;
            }

            var requestedLine = requestedByProduct[existingLine.ProductId];

            sale.ChangeLineQuantity(
                existingLine.Id,
                requestedLine.Quantity);

            sale.ChangeLineUnitSellingPrice(
                existingLine.Id,
                new Money(requestedLine.UnitSellingPrice));
        }

        var existingProductIds = sale.Lines
            .Select(x => x.ProductId)
            .ToHashSet();

        foreach (var requestedLine in request.Lines)
        {
            if (existingProductIds.Contains(requestedLine.ProductId))
                continue;

            sale.AddLine(
                new SaleLine(
                    sale.Id,
                    requestedLine.ProductId,
                    requestedLine.Quantity,
                    new Money(requestedLine.UnitSellingPrice)));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
