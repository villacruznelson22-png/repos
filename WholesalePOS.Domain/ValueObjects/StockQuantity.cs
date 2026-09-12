using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class StockQuantity : ValueObject
{
    public decimal Value { get; }

    public StockQuantity(decimal value)
    {
        if (value < 0)
            throw new ArgumentException(
                "Stock quantity cannot be negative.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static StockQuantity operator +(
        StockQuantity stock,
        decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException(
                "Quantity to add cannot be negative.");

        return new StockQuantity(
            stock.Value + quantity);
    }

    public static StockQuantity operator -(
        StockQuantity stock,
        decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException(
                "Quantity to remove cannot be negative.");

        if (quantity > stock.Value)
            throw new InvalidOperationException(
                "Insufficient stock.");

        return new StockQuantity(
            stock.Value - quantity);
    }
    public override string ToString()
    {
        return Value.ToString();
    }
}