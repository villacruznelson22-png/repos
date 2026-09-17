using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderHandler
    : IRequestHandler<CancelPurchaseOrderCommand>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelPurchaseOrderCommand request,
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

        purchaseOrder.Cancel();

        _purchaseOrderRepository.Update(purchaseOrder);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}