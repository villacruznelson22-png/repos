using MediatR;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;

public record CreateDeliveryReceiptCommand(
    Guid PurchaseOrderId,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    IReadOnlyCollection<CreateDeliveryReceiptLineRequest> Lines
) : IRequest<Guid>;

public record CreateDeliveryReceiptLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost,
    DateTime? ExpirationDate
);