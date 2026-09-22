using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;

public class CreateDeliveryReceiptHandler
    : IRequestHandler<CreateDeliveryReceiptCommand, Guid>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeliveryReceiptHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProductRepository productRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _productRepository = productRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateDeliveryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder =
            await _purchaseOrderRepository.GetByIdWithLinesAsync(
                request.PurchaseOrderId,
                cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundException(
                $"Purchase order '{request.PurchaseOrderId}' was not found.");
        }

        if (purchaseOrder.Status != Domain.Enums.PurchaseOrderStatus.Posted)
        {
            throw new InvalidOperationException(
                "A delivery receipt can only be created from a posted purchase order.");
        }

        var productIds = request.Lines
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

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
                    $"Product '{productId}' is inactive and cannot be received.");
            }
        }

        var deliveryReceipt = new DeliveryReceipt(
            request.PurchaseOrderId,
            request.OccurredAt,
            request.ReferenceNumber,
            request.Notes);

        foreach (var requestLine in request.Lines)
        {
            var line = new DeliveryReceiptLine(
                deliveryReceipt.Id,
                requestLine.ProductId,
                requestLine.Quantity,
                new Money(requestLine.UnitCost),
                requestLine.ExpirationDate);

            deliveryReceipt.AddLine(line);
        }

        await _deliveryReceiptRepository.AddAsync(
            deliveryReceipt,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return deliveryReceipt.Id;
    }
}