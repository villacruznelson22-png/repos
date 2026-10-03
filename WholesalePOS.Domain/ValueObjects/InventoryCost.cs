using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class InventoryCost : ValueObject
{
    private const int Precision = 6;

    public decimal Value { get; }

    public InventoryCost(decimal value)
    {
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
