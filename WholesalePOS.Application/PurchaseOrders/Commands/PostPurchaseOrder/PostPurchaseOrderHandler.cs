using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;

public class PostPurchaseOrderHandler
    : IRequestHandler<PostPurchaseOrderCommand>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostPurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        PostPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder =
            await _purchaseOrderRepository.GetByIdWithLinesAsync(
                request.Id,
                cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundException(
                $"Purchase order '{request.Id}' was not found.");
        }

        purchaseOrder.Post();

        _purchaseOrderRepository.Update(purchaseOrder);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}