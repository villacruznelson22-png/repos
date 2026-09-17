using MediatR;

namespace WholesalePOS.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public record CancelPurchaseOrderCommand(
    Guid Id
) : IRequest;