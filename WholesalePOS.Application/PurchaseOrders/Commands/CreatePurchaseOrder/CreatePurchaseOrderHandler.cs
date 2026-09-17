using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderHandler
    : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.SupplierId,
            cancellationToken);

        if (supplier is null)
        {
            throw SupplierErrors.NotFound(request.SupplierId);
        }

        if (!supplier.IsActive)
        {
            throw new InvalidOperationException(
                "An inactive supplier cannot be used for a new purchase order.");
        }

        var purchaseOrder = new PurchaseOrder(
            request.SupplierId,
            request.OccurredAt,
            request.ReferenceNumber,
            request.Notes);

        foreach (var requestLine in request.Lines)
        {
            var line = new PurchaseOrderLine(
                purchaseOrder.Id,
                requestLine.ProductId,
                requestLine.Quantity,
                new Money(requestLine.UnitCost));

            purchaseOrder.AddLine(line);
        }

        await _purchaseOrderRepository.AddAsync(
            purchaseOrder,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return purchaseOrder.Id;
    }
}