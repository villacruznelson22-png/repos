using MediatR;

namespace WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public record UpdatePurchaseOrderCommand(
    Guid Id,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    IReadOnlyCollection<UpdatePurchaseOrderLineRequest> Lines
) : IRequest;

public record UpdatePurchaseOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost
);