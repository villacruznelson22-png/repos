using MediatR;

namespace WholesalePOS.Application.DeliveryReceipts.Queries.GetDeliveryReceiptById;

public record GetDeliveryReceiptByIdQuery(
    Guid Id
) : IRequest<DeliveryReceiptDto>;