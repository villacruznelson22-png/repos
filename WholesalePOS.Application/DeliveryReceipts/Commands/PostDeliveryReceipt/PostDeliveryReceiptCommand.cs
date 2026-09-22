using MediatR;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;

public record PostDeliveryReceiptCommand(
    Guid Id
) : IRequest;