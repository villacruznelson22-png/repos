using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.PurchaseOrders;

public class PurchaseOrderLineTests
{
    [Fact]
    public void Constructor_ShouldCreateValidLine()
    {
        var purchaseOrderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var line = new PurchaseOrderLine(
            purchaseOrderId,
            productId,
            10,
            new Money(180));

        Assert.NotEqual(Guid.Empty, line.Id);
        Assert.Equal(purchaseOrderId, line.PurchaseOrderId);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(10, line.Quantity);
        Assert.Equal(180, line.UnitCost.Value);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPurchaseOrderIdIsEmpty()
    {
        var exception = Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrderLine(
                Guid.Empty,
                Guid.NewGuid(),
                10,
                new Money(180)));

        Assert.Equal(
            "Purchase order ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        var exception = Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrderLine(
                Guid.NewGuid(),
                Guid.Empty,
                10,
                new Money(180)));

        Assert.Equal(
            "Product ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrderLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsNegative()
    {
        Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrderLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                -1,
                new Money(180)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitCostIsNegative()
    {
        Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrderLine(
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                new Money(-1)));
    }

    [Fact]
    public void ChangeQuantity_ShouldUpdateQuantity()
    {
        var line = new PurchaseOrderLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(180));

        line.ChangeQuantity(20);

        Assert.Equal(20, line.Quantity);
    }

    [Fact]
    public void ChangeQuantity_ShouldThrow_WhenQuantityIsInvalid()
    {
        var line = new PurchaseOrderLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(180));

        Assert.Throws<PurchaseOrderDomainException>(
            () => line.ChangeQuantity(0));
    }

    [Fact]
    public void ChangeUnitCost_ShouldUpdateUnitCost()
    {
        var line = new PurchaseOrderLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            new Money(180));

        line.ChangeUnitCost(new Money(185));

        Assert.Equal(185, line.UnitCost.Value);
    }
}