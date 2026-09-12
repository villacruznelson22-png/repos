using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class StockQuantityTests
{
    [Fact]
    public void ShouldCreate_WhenQuantityIsValid()
    {
        var stock = new StockQuantity(100);

        Assert.Equal(100, stock.Value);
    }

    [Fact]
    public void ShouldThrow_WhenQuantityIsNegative()
    {
        var action = () =>
            new StockQuantity(-1);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ShouldIncreaseStock()
    {
        var stock = new StockQuantity(100);

        var result = stock + 25;

        Assert.Equal(
            new StockQuantity(125),
            result);
    }

    [Fact]
    public void ShouldDecreaseStock()
    {
        var stock = new StockQuantity(100);

        var result = stock - 25;

        Assert.Equal(
            new StockQuantity(75),
            result);
    }

    [Fact]
    public void ShouldThrow_WhenRemovingMoreThanAvailable()
    {
        var stock = new StockQuantity(100);

        var action = () =>
            stock - 101;

        Assert.Throws<InvalidOperationException>(action);
    }
}