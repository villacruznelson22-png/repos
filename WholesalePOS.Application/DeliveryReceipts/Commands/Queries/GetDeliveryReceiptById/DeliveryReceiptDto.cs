namespace WholesalePOS.Application.DeliveryReceipts.Queries.GetDeliveryReceiptById;

public record DeliveryReceiptDto(
    Guid Id,
    Guid PurchaseOrderId,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    int Status,
    DateTime CreatedAt,
    IReadOnlyCollection<DeliveryReceiptLineDto> Lines
);

public record DeliveryReceiptLineDto(
    Guid Id,
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost,
    DateTime? ExpirationDate
);