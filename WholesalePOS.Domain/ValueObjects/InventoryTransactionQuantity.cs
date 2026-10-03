using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class InventoryTransactionQuantity : ValueObject
{
    public decimal Value { get; }

    public InventoryTransactionQuantity(decimal value)
    {
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
