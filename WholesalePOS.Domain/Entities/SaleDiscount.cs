using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

/// <summary>
/// An immutable record of a discount actually applied to a sale.
/// Policy/eligibility rules are intentionally modeled separately later.
/// </summary>
public class SaleDiscount
{
    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public Guid? SaleLineId { get; private set; }
    public DiscountScope Scope { get; private set; }
    public DiscountCalculationType CalculationType { get; private set; }

    /// <summary>
    /// Percentage points (for example, 10 means 10%) or fixed currency amount.
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Calculated and persisted amount in currency, preserving the transaction history.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    public string Reason { get; private set; } = null!;
    public Guid AppliedByUserId { get; private set; }
    public DateTime AppliedAt { get; private set; }

    public Sale Sale { get; private set; } = null!;
    public SaleLine? SaleLine { get; private set; }
    public User AppliedByUser { get; private set; } = null!;

    private SaleDiscount() { }

    public SaleDiscount(
        Guid saleId,
        Guid? saleLineId,
        DiscountScope scope,
        DiscountCalculationType calculationType,
        decimal value,
        Money amount,
        string reason,
        Guid appliedByUserId)
    {
        if (saleId == Guid.Empty)
            throw new SaleDomainException("Sale ID cannot be empty.");

        if (scope == DiscountScope.SaleLine && (!saleLineId.HasValue || saleLineId == Guid.Empty))
            throw new SaleDomainException("A line-item discount must reference a sale line.");

        if (scope == DiscountScope.Sale && saleLineId.HasValue)
            throw new SaleDomainException("A sale-wide discount cannot reference a sale line.");

        if (!Enum.IsDefined(scope))
            throw new SaleDomainException("Discount scope is invalid.");

        if (!Enum.IsDefined(calculationType))
            throw new SaleDomainException("Discount calculation type is invalid.");

        if (value <= 0)
            throw new SaleDomainException("Discount value must be greater than zero.");

        if (calculationType == DiscountCalculationType.Percentage && value > 100)
            throw new SaleDomainException("Percentage discount cannot exceed 100%.");

        if (amount is null || amount.Value <= 0)
            throw new SaleDomainException("Calculated discount amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new SaleDomainException("Discount reason is required.");

        if (appliedByUserId == Guid.Empty)
            throw new SaleDomainException("The user applying the discount is required.");

        Id = Guid.NewGuid();
        SaleId = saleId;
        SaleLineId = saleLineId;
        Scope = scope;
        CalculationType = calculationType;
        Value = value;
        Amount = amount;
        Reason = reason.Trim();
        AppliedByUserId = appliedByUserId;
        AppliedAt = DateTime.UtcNow;
    }
}
