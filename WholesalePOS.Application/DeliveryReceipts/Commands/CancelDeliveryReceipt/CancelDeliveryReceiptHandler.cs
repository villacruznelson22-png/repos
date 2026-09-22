using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.CancelDeliveryReceipt;

public class CancelDeliveryReceiptHandler
    : IRequestHandler<CancelDeliveryReceiptCommand>
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelDeliveryReceiptHandler(
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IUnitOfWork unitOfWork)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelDeliveryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var deliveryReceipt =
            await _deliveryReceiptRepository.GetByIdWithLinesAsync(
                request.Id,
                cancellationToken);

        if (deliveryReceipt is null)
        {
            throw new NotFoundException(
                $"Delivery receipt '{request.Id}' was not found.");
        }

        deliveryReceipt.Cancel();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}