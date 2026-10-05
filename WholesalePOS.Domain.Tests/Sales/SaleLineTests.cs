using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Sales;

public class SaleLineTests
{
    private static SaleLine CreateSaleLine(
        Guid? saleId = null,
        Guid? productId = null,
        decimal quantity = 10,
        Money? unitSellingPrice = null)
    {
        return new SaleLine(
            saleId ?? Guid.NewGuid(),
            productId ?? Guid.NewGuid(),
            quantity,
            unitSellingPrice ?? new Money(180));
    }

    [Fact]
    public void Constructor_ShouldCreateSaleLine()
    {
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var line = new SaleLine(
            saleId,
            productId,
            10,
            new Money(180));

        Assert.NotEqual(
            Guid.Empty,
            line.Id);

        Assert.Equal(
            saleId,
            line.SaleId);

        Assert.Equal(
            productId,
            line.ProductId);

        Assert.Equal(
            10,
            line.Quantity);

        Assert.Equal(
            180,
            line.UnitSellingPrice.Value);

        Assert.Null(line.UnitCost);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSaleIdIsEmpty()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.Empty,
                Guid.NewGuid(),
                10,
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.NewGuid(),
                Guid.Empty,
                10,
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        var exception =
            Assert.Throws<SaleDomainException>(
                () => CreateSaleLine(quantity: 0));

        Assert.Equal(
            "Sale quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsNegative()
    {
        var exception =
            Assert.Throws<SaleDomainException>(
                () => CreateSaleLine(quantity: -1));

        Assert.Equal(
            "Sale quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSellingPriceIsNegative()
    {
        var exception =
            Assert.Throws<SaleDomainException>(
                () => CreateSaleLine(
                    unitSellingPrice: new Money(-1)));

        Assert.Equal(
            "Sale unit selling price cannot be negative.",
            exception.Message);
    }

    [Fact]
    public void ChangeQuantity_ShouldChangeQuantity()
    {
        var line = CreateSaleLine();

        line.ChangeQuantity(25);

        Assert.Equal(
            25,
            line.Quantity);
    }

    [Fact]
    public void ChangeQuantity_ShouldThrow_WhenQuantityIsZero()
    {
        var line = CreateSaleLine();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => line.ChangeQuantity(0));

        Assert.Equal(
            "Sale quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void ChangeQuantity_ShouldThrow_WhenQuantityIsNegative()
    {
        var line = CreateSaleLine();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => line.ChangeQuantity(-5));

        Assert.Equal(
            "Sale quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void ChangeUnitSellingPrice_ShouldChangePrice()
    {
        var line = CreateSaleLine();

        line.ChangeUnitSellingPrice(
            new Money(195));

        Assert.Equal(
            195,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void ChangeUnitSellingPrice_ShouldThrow_WhenPriceIsNegative()
    {
        var line = CreateSaleLine();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => line.ChangeUnitSellingPrice(
                    new Money(-10)));

        Assert.Equal(
            "Sale unit selling price cannot be negative.",
            exception.Message);
    }

    [Fact]
    public void SetUnitCost_ShouldCaptureInventoryCost()
    {
        var line = CreateSaleLine();

        var cost = new InventoryCost(123.456789m);

        line.SetUnitCost(cost);

        Assert.NotNull(line.UnitCost);

        Assert.Equal(
            123.456789m,
            line.UnitCost!.Value);
    }

    [Fact]
    public void SetUnitCost_ShouldPreserveInventoryCostPrecision()
    {
        var line = CreateSaleLine();

        var cost = new InventoryCost(123.123456789m);

        line.SetUnitCost(cost);

        Assert.Equal(
            123.123457m,
            line.UnitCost!.Value);
    }

    [Fact]
    public void SetUnitCost_ShouldThrow_WhenCostIsAlreadySet()
    {
        var line = CreateSaleLine();

        line.SetUnitCost(
            new InventoryCost(100));

        var exception =
            Assert.Throws<SaleDomainException>(
                () => line.SetUnitCost(
                    new InventoryCost(120)));

        Assert.Equal(
            "Sale unit cost has already been captured.",
            exception.Message);

        Assert.Equal(
            100,
            line.UnitCost!.Value);
    }

    [Fact]
    public void ChangeUnitSellingPrice_ShouldNotChangeUnitCost()
    {
        var line = CreateSaleLine();

        line.SetUnitCost(
            new InventoryCost(123.456789m));

        line.ChangeUnitSellingPrice(
            new Money(200));

        Assert.Equal(
            123.456789m,
            line.UnitCost!.Value);

        Assert.Equal(
            200,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void ChangeQuantity_ShouldNotChangeUnitCost()
    {
        var line = CreateSaleLine();

        line.SetUnitCost(
            new InventoryCost(123.456789m));

        line.ChangeQuantity(25);

        Assert.Equal(
            25,
            line.Quantity);

        Assert.Equal(
            123.456789m,
            line.UnitCost!.Value);
    }
}