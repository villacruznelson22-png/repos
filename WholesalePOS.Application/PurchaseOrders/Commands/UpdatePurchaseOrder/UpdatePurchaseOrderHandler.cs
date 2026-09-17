using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public class UpdatePurchaseOrderHandler
    : IRequestHandler<UpdatePurchaseOrderCommand>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdatePurchaseOrderCommand request,
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

        purchaseOrder.ChangeOccurredAt(
            request.OccurredAt);

        purchaseOrder.ChangeReferenceNumber(
            request.ReferenceNumber);

        purchaseOrder.ChangeNotes(
            request.Notes);

        var incomingProductIds = request.Lines
            .Select(x => x.ProductId)
            .ToHashSet();

        foreach (var existingLine in purchaseOrder.Lines.ToList())
        {
            if (!incomingProductIds.Contains(existingLine.ProductId))
            {
                purchaseOrder.RemoveLine(existingLine.Id);
            }
        }

        foreach (var requestLine in request.Lines)
        {
            var existingLine = purchaseOrder.Lines
                .SingleOrDefault(x =>
                    x.ProductId == requestLine.ProductId);

            if (existingLine is null)
            {
                var newLine = new PurchaseOrderLine(
                    purchaseOrder.Id,
                    requestLine.ProductId,
                    requestLine.Quantity,
                    new Money(requestLine.UnitCost));

                purchaseOrder.AddLine(newLine);
            }
            else
            {
                purchaseOrder.ChangeLineQuantity(
                    existingLine.Id,
                    requestLine.Quantity);

                purchaseOrder.ChangeLineUnitCost(
                    existingLine.Id,
                    new Money(requestLine.UnitCost));
            }
        }

        _purchaseOrderRepository.Update(purchaseOrder);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}