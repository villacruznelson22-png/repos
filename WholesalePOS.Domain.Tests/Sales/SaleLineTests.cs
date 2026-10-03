using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Sales;

public class SaleLineTests
{
    [Fact]
    public void Constructor_ShouldCreateValidLine()
    {
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var line = new SaleLine(
            saleId,
            productId,
            10,
            new Money(150),
            new Money(180));

        Assert.NotEqual(Guid.Empty, line.Id);

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
            150,
            line.UnitCost.Value);

        Assert.Equal(
            180,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSaleIdIsEmpty()
    {
        var exception =
            Assert.Throws<SaleDomainException>(
                () => new SaleLine(
                    Guid.Empty,
                    Guid.NewGuid(),
                    10,
                    new Money(150),
                    new Money(180)));

        Assert.Equal(
            "Sale ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        var exception =
            Assert.Throws<SaleDomainException>(
                () => new SaleLine(
                    Guid.NewGuid(),
                    Guid.Empty,
                    10,
                    new Money(150),
                    new Money(180)));

        Assert.Equal(
            "Product ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                new Money(150),
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsNegative()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                -1,
                new Money(150),
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitCostIsNegative()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                new Money(-1),
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitSellingPriceIsNegative()
    {
        Assert.Throws<SaleDomainException>(
            () => new SaleLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                new Money(150),
                new Money(-1)));
    }

    [Fact]
    public void ChangeQuantity_ShouldUpdateQuantity()
    {
        var line = new SaleLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(150),
            new Money(180));

        line.ChangeQuantity(20);

        Assert.Equal(
            20,
            line.Quantity);
    }

    [Fact]
    public void ChangeQuantity_ShouldThrow_WhenQuantityIsInvalid()
    {
        var line = new SaleLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(150),
            new Money(180));

        Assert.Throws<SaleDomainException>(
            () => line.ChangeQuantity(0));
    }

    [Fact]
    public void ChangeUnitSellingPrice_ShouldUpdatePrice()
    {
        var line = new SaleLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(150),
            new Money(180));

        line.ChangeUnitSellingPrice(
            new Money(185));

        Assert.Equal(
            185,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void ChangeUnitSellingPrice_ShouldThrow_WhenPriceIsNegative()
    {
        var line = new SaleLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(150),
            new Money(180));

        Assert.Throws<SaleDomainException>(
            () => line.ChangeUnitSellingPrice(
                new Money(-1)));
    }

    [Fact]
    public void UnitCost_ShouldRemainUnchanged_WhenSellingPriceChanges()
    {
        var line = new SaleLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(150),
            new Money(180));

        line.ChangeUnitSellingPrice(
            new Money(190));

        Assert.Equal(
            150,
            line.UnitCost.Value);

        Assert.Equal(
            190,
            line.UnitSellingPrice.Value);
    }
}