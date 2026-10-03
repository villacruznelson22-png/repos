
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

/// <summary>
/// Represents one product line within a sale.
/// </summary>
public class SaleLine
{
    public Guid Id { get; private set; }

    public Guid SaleId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>
    /// Historical inventory cost captured when the sale is created.
    /// This is system-controlled and must not be edited by sales users.
    /// </summary>
    public Money UnitCost { get; private set; } = null!;

    /// <summary>
    /// Historical selling price captured when the sale is created.
    /// This may be changed only through an authorized application operation
    /// while the sale is still in Draft status.
    /// </summary>
    public Money UnitSellingPrice { get; private set; } = null!;

    public Sale Sale { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    private SaleLine()
    {
    }

    public SaleLine(
        Guid saleId,
        Guid productId,
        decimal quantity,
        Money unitCost,
        Money unitSellingPrice)
    {
        if (saleId == Guid.Empty)
            throw new SaleDomainException("Sale ID cannot be empty.");

        if (productId == Guid.Empty)
            throw new SaleDomainException("Product ID cannot be empty.");

        if (quantity <= 0)
            throw new SaleDomainException(
                "Sale quantity must be greater than zero.");

        if (unitCost.Value < 0)
            throw new SaleDomainException(
                "Sale unit cost cannot be negative.");

        if (unitSellingPrice.Value < 0)
            throw new SaleDomainException(
                "Sale unit selling price cannot be negative.");

        Id = Guid.NewGuid();
        SaleId = saleId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        UnitSellingPrice = unitSellingPrice;
    }

    public void ChangeQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new SaleDomainException(
                "Sale quantity must be greater than zero.");

        Quantity = quantity;
    }

    public void ChangeUnitSellingPrice(Money unitSellingPrice)
    {
        if (unitSellingPrice.Value < 0)
            throw new SaleDomainException(
                "Sale unit selling price cannot be negative.");

        UnitSellingPrice = unitSellingPrice;
    }
}