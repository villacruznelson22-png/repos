using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithFallbackInventoryCost_ShouldStoreCost()
    {
        var fallbackCost = new InventoryCost(65.123456m);

        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(75),
            new Money(70),
            fallbackCost);

        Assert.NotNull(product.FallbackInventoryCost);
        Assert.Equal(65.123456m, product.FallbackInventoryCost.Value.Value);
    }

    [Fact]
    public void CreateProduct_WithoutFallbackInventoryCost_ShouldHaveNullFallbackCost()
    {
        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(75),
            new Money(70));

        Assert.Null(product.FallbackInventoryCost);
    }

    [Fact]
    public void ChangeFallbackInventoryCost_ShouldUpdateFallbackCost()
    {
        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(75),
            new Money(70));

        product.ChangeFallbackInventoryCost(
            new InventoryCost(68.500001m));

        Assert.NotNull(product.FallbackInventoryCost);
        Assert.Equal(
            68.500001m,
            product.FallbackInventoryCost.Value.Value);
    }

    [Fact]
    public void ChangeFallbackInventoryCost_WithNull_ShouldClearFallbackCost()
    {
        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(75),
            new Money(70),
            new InventoryCost(65));

        product.ChangeFallbackInventoryCost(null);

        Assert.Null(product.FallbackInventoryCost);
    }
}
