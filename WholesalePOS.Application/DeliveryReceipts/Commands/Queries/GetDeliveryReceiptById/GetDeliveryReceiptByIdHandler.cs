using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.DeliveryReceipts.Queries.GetDeliveryReceiptById;

public class GetDeliveryReceiptByIdHandler
    : IRequestHandler<GetDeliveryReceiptByIdQuery, DeliveryReceiptDto>
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    public GetDeliveryReceiptByIdHandler(
        IDeliveryReceiptRepository deliveryReceiptRepository)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
    }

    public async Task<DeliveryReceiptDto> Handle(
        GetDeliveryReceiptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryReceipt =
            await _deliveryReceiptRepository.GetByIdWithLinesAsync(
                request.Id,
                cancellationToken);

        if (deliveryReceipt is null)
        {
            throw new NotFoundException(
                $"Delivery receipt '{request.Id}' was not found.");
        }

        return new DeliveryReceiptDto(
            deliveryReceipt.Id,
            deliveryReceipt.PurchaseOrderId,
            deliveryReceipt.OccurredAt,
            deliveryReceipt.ReferenceNumber,
            deliveryReceipt.Notes,
            (int)deliveryReceipt.Status,
            deliveryReceipt.CreatedAt,
            deliveryReceipt.Lines
                .Select(line => new DeliveryReceiptLineDto(
                    line.Id,
                    line.ProductId,
                    line.Quantity,
                    line.UnitCost.Value,
                    line.ExpirationDate))
                .ToList());
    }
}