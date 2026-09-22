using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.DeliveryReceipts;

public class DeliveryReceiptLineTests
{
    [Fact]
    public void Constructor_ShouldCreateDeliveryReceiptLine()
    {
        var deliveryReceiptId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var expirationDate = new DateTime(
            2026,
            12,
            31);

        var line = new DeliveryReceiptLine(
            deliveryReceiptId,
            productId,
            20,
            new Money(180),
            expirationDate);

        Assert.NotEqual(
            Guid.Empty,
            line.Id);

        Assert.Equal(
            deliveryReceiptId,
            line.DeliveryReceiptId);

        Assert.Equal(
            productId,
            line.ProductId);

        Assert.Equal(
            20,
            line.Quantity);

        Assert.Equal(
            180,
            line.UnitCost.Value);

        Assert.Equal(
            expirationDate,
            line.ExpirationDate);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDeliveryReceiptIdIsEmpty()
    {
        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => new DeliveryReceiptLine(
                    Guid.Empty,
                    Guid.NewGuid(),
                    10,
                    new Money(180)));

        Assert.Equal(
            "Delivery receipt ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => new DeliveryReceiptLine(
                    Guid.NewGuid(),
                    Guid.Empty,
                    10,
                    new Money(180)));

        Assert.Equal(
            "Product ID cannot be empty.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenQuantityIsNotPositive(
        decimal quantity)
    {
        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => new DeliveryReceiptLine(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    quantity,
                    new Money(180)));

        Assert.Equal(
            "Delivery receipt quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitCostIsNegative()
    {
        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => new DeliveryReceiptLine(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    10,
                    new Money(-1)));

        Assert.Equal(
            "Delivery receipt unit cost cannot be negative.",
            exception.Message);
    }

    [Fact]
    public void ChangeQuantity_ShouldUpdateQuantity()
    {
        var line = CreateLine();

        line.ChangeQuantity(25);

        Assert.Equal(
            25,
            line.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeQuantity_ShouldThrow_WhenQuantityIsNotPositive(
        decimal quantity)
    {
        var line = CreateLine();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => line.ChangeQuantity(quantity));

        Assert.Equal(
            "Delivery receipt quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void ChangeUnitCost_ShouldUpdateUnitCost()
    {
        var line = CreateLine();

        line.ChangeUnitCost(
            new Money(195));

        Assert.Equal(
            195,
            line.UnitCost.Value);
    }

    [Fact]
    public void ChangeUnitCost_ShouldThrow_WhenUnitCostIsNegative()
    {
        var line = CreateLine();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => line.ChangeUnitCost(
                    new Money(-1)));

        Assert.Equal(
            "Delivery receipt unit cost cannot be negative.",
            exception.Message);
    }

    [Fact]
    public void ChangeExpirationDate_ShouldUpdateExpirationDate()
    {
        var line = CreateLine();

        var expirationDate = new DateTime(
            2027,
            1,
            15);

        line.ChangeExpirationDate(
            expirationDate);

        Assert.Equal(
            expirationDate,
            line.ExpirationDate);
    }

    [Fact]
    public void ChangeExpirationDate_ShouldAllowNull()
    {
        var line = new DeliveryReceiptLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(180),
            new DateTime(2026, 12, 31));

        line.ChangeExpirationDate(null);

        Assert.Null(
            line.ExpirationDate);
    }

    private static DeliveryReceiptLine CreateLine()
    {
        return new DeliveryReceiptLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(180));
    }
}