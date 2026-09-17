using MediatR;

namespace WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;

public record PostPurchaseOrderCommand(
    Guid Id
) : IRequest;