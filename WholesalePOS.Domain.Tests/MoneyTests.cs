using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void ShouldRoundToTwoDecimalPlaces()
    {
        var money = new Money(25.567m);

        Assert.Equal(25.57m, money.Value);
    }

    [Fact]
    public void ShouldBeEqual_WhenValuesAreTheSame()
    {
        var first = new Money(25.50m);
        var second = new Money(25.50m);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ShouldAddTwoMoneyValues()
    {
        var first = new Money(25);
        var second = new Money(30);

        var result = first + second;

        Assert.Equal(new Money(55), result);
    }

    [Fact]
    public void ShouldSubtractTwoMoneyValues()
    {
        var first = new Money(30);
        var second = new Money(25);

        var result = first - second;

        Assert.Equal(new Money(5), result);
    }

    [Fact]
    public void ShouldMultiplyMoneyByQuantity()
    {
        var money = new Money(25);

        var result = money * 4;

        Assert.Equal(new Money(100), result);
    }
}