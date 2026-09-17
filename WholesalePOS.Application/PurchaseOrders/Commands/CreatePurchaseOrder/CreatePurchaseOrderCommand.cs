using MediatR;

namespace WholesalePOS.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    IReadOnlyCollection<CreatePurchaseOrderLineRequest> Lines
) : IRequest<Guid>;

public record CreatePurchaseOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost
);