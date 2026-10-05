using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class SaleLine
{
    public Guid Id { get; private set; }

    public Guid SaleId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>
    /// Historical inventory cost captured when the sale is completed.
    /// This is system-controlled and remains null while the sale is
    /// still Draft or Confirmed.
    /// </summary>
    public InventoryCost? UnitCost { get; private set; }

    /// <summary>
    /// Historical selling price captured for this sale line.
    /// This can be changed while the sale is still editable.
    /// </summary>
    public Money UnitSellingPrice { get; private set; } = null!;

    public Sale Sale { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    private SaleLine()
    {
        // Used by EF Core
    }

    public SaleLine(
        Guid saleId,
        Guid productId,
        decimal quantity,
        Money unitSellingPrice)
    {
        if (saleId == Guid.Empty)
            throw new SaleDomainException(
                "Sale ID cannot be empty.");

        if (productId == Guid.Empty)
            throw new SaleDomainException(
                "Product ID cannot be empty.");

        if (quantity <= 0)
            throw new SaleDomainException(
                "Sale quantity must be greater than zero.");

        if (unitSellingPrice.Value < 0)
            throw new SaleDomainException(
                "Sale unit selling price cannot be negative.");

        Id = Guid.NewGuid();

        SaleId = saleId;
        ProductId = productId;
        Quantity = quantity;
        UnitSellingPrice = unitSellingPrice;
    }

    public void ChangeQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new SaleDomainException(
                "Sale quantity must be greater than zero.");

        Quantity = quantity;
    }

    public void ChangeUnitSellingPrice(
        Money unitSellingPrice)
    {
        if (unitSellingPrice.Value < 0)
            throw new SaleDomainException(
                "Sale unit selling price cannot be negative.");

        UnitSellingPrice = unitSellingPrice;
    }

    /// <summary>
    /// Captures the historical inventory cost at checkout.
    /// This can only happen once.
    /// </summary>
    public void SetUnitCost(InventoryCost unitCost)
    {
        if (UnitCost is not null)
            throw new SaleDomainException(
                "Sale unit cost has already been captured.");

        UnitCost = unitCost;
    }
}