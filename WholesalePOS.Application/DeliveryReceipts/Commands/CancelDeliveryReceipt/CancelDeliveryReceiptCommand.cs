using MediatR;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.CancelDeliveryReceipt;

public record CancelDeliveryReceiptCommand(
    Guid Id
) : IRequest;