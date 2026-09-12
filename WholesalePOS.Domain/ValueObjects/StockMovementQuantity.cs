using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class StockMovementQuantity : ValueObject
{
    public decimal Value { get; }

    public StockMovementQuantity(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException(
                "Stock movement quantity must be greater than zero.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}