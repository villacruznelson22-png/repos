using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;

public class PostDeliveryReceiptHandler
    : IRequestHandler<PostDeliveryReceiptCommand>
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostDeliveryReceiptHandler(
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IUnitOfWork unitOfWork)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        PostDeliveryReceiptCommand request,
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

        deliveryReceipt.Post();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}