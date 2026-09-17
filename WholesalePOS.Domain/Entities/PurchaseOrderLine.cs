using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class PurchaseOrderLine
{
    public Guid Id { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public Money UnitCost { get; private set; } = null!;

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    private PurchaseOrderLine()
    {
        // Used by EF Core
    }

    public PurchaseOrderLine(
        Guid purchaseOrderId,
        Guid productId,
        decimal quantity,
        Money unitCost)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new PurchaseOrderDomainException(
                "Purchase order ID cannot be empty.");

        if (productId == Guid.Empty)
            throw new PurchaseOrderDomainException(
                "Product ID cannot be empty.");

        if (quantity <= 0)
            throw new PurchaseOrderDomainException(
                "Purchase order quantity must be greater than zero.");

        if (unitCost.Value < 0)
            throw new PurchaseOrderDomainException(
                "Purchase order unit cost cannot be negative.");

        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
    }

    public void ChangeQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new PurchaseOrderDomainException(
                "Purchase order quantity must be greater than zero.");

        Quantity = quantity;
    }

    public void ChangeUnitCost(Money unitCost)
    {
        if (unitCost.Value < 0)
            throw new PurchaseOrderDomainException(
                "Purchase order unit cost cannot be negative.");

        UnitCost = unitCost;
    }
}