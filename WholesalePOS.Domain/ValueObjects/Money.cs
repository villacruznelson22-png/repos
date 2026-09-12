using WholesalePOS.Domain.Common;

namespace WholesalePOS.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public override string ToString()
    {
        return Value.ToString("0.00");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Value + right.Value);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Value - right.Value);
    }

    public static Money operator *(Money money, int quantity)
    {
        return new Money(
            money.Value * quantity);
    }


    public static bool operator >(Money left, Money right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <(Money left, Money right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >=(Money left, Money right)
    {
        return left.Value >= right.Value;
    }

    public static bool operator <=(Money left, Money right)
    {
        return left.Value <= right.Value;
    }

  
}