using MediatR;

namespace WholesalePOS.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(
    Guid Id
) : IRequest<PurchaseOrderDto>;