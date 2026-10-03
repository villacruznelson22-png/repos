using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

/// <summary>
/// Represents a positive quantity recorded by an inventory transaction.
/// </summary>
public sealed class InventoryTransactionQuantity : ValueObject
{
    /// <summary>
    /// Quantity affected by the inventory transaction.
    /// </summary>
    public decimal Value { get; }

    public InventoryTransactionQuantity(decimal value)
    {
        // BUSINESS RULE: transaction quantities are always positive.
        // Increase/decrease is represented separately by Direction.
        if (value <= 0)
            throw new ArgumentException(
                "Inventory transaction quantity must be greater than zero.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("0.###");
}
