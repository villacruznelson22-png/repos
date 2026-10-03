using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

/// <summary>
/// Represents an inventory unit cost using six decimal places of internal precision.
/// Higher precision prevents repeated MWAC calculations from accumulating
/// avoidable rounding error.
/// </summary>
public sealed class InventoryCost : ValueObject
{
    private const int Precision = 6;

    /// <summary>
    /// Numeric inventory cost per unit.
    /// </summary>
    public decimal Value { get; }

    public InventoryCost(decimal value)
    {
        // BUSINESS RULE: inventory cost cannot be negative.
        if (value < 0)
            throw new ArgumentException(
                "Inventory cost cannot be negative.");

        Value = decimal.Round(
            value,
            Precision,
            MidpointRounding.AwayFromZero);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString("0.######");
    }
}
