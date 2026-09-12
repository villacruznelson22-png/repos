using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class StockMovementQuantityTests
{
    [Fact]
    public void ShouldCreate_WhenQuantityIsPositive()
    {
        var quantity =
            new StockMovementQuantity(10);

        Assert.Equal(10, quantity.Value);
    }

    [Fact]
    public void ShouldSupportFractionalQuantity()
    {
        var quantity =
            new StockMovementQuantity(0.5m);

        Assert.Equal(0.5m, quantity.Value);
    }

    [Fact]
    public void ShouldThrow_WhenQuantityIsZero()
    {
        var action = () =>
            new StockMovementQuantity(0);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ShouldThrow_WhenQuantityIsNegative()
    {
        var action = () =>
            new StockMovementQuantity(-1);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ShouldBeEqual_WhenValuesAreTheSame()
    {
        var first =
            new StockMovementQuantity(0.5m);

        var second =
            new StockMovementQuantity(0.5m);

        Assert.Equal(first, second);
    }
}