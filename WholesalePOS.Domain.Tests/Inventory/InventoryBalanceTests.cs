using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Inventory;

public class InventoryBalanceTests
{
    [Fact]
    public void CreateOpeningBalance_ShouldSetQuantityAndAverageCost()
    {
        var productId = Guid.NewGuid();

        var inventory = InventoryBalance.CreateOpeningBalance(
            productId,
            100,
            new InventoryCost(65));

        Assert.Equal(100, inventory.QuantityOnHand);
        Assert.Equal(6500, inventory.InventoryValue);
        Assert.Equal(65, inventory.AverageUnitCost.Value);
    }

    [Fact]
    public void Receive_ShouldCalculateMovingWeightedAverage()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            100,
            new InventoryCost(65));

        inventory.Receive(
            50,
            new InventoryCost(70));

        Assert.Equal(150, inventory.QuantityOnHand);
        Assert.Equal(10000, inventory.InventoryValue);
        Assert.Equal(
            66.666667m,
            inventory.AverageUnitCost.Value);
    }

    [Fact]
    public void Consume_ShouldUseCurrentAverageCost()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            100,
            new InventoryCost(65));

        inventory.Receive(
            50,
            new InventoryCost(70));

        var cost = inventory.Consume(10);

        Assert.Equal(
            66.666667m,
            cost.Value);

        Assert.Equal(140, inventory.QuantityOnHand);
        Assert.Equal(9333.333330m, inventory.InventoryValue);
        Assert.Equal(
            66.666667m,
            inventory.AverageUnitCost.Value);
    }

    [Fact]
    public void Consume_ShouldNotChangeAverageCost()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            100,
            new InventoryCost(65));

        inventory.Receive(
            50,
            new InventoryCost(70));

        inventory.Consume(25);

        Assert.Equal(
            66.666667m,
            inventory.AverageUnitCost.Value);
    }

    [Fact]
    public void Consume_AllInventory_ShouldResetValueAndAverageCost()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            10,
            new InventoryCost(65));

        inventory.Consume(10);

        Assert.Equal(0, inventory.QuantityOnHand);
        Assert.Equal(0, inventory.InventoryValue);
        Assert.Equal(0, inventory.AverageUnitCost.Value);
    }

    [Fact]
    public void Consume_ShouldRejectInsufficientInventory()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            5,
            new InventoryCost(65));

        var action = () => inventory.Consume(6);

        Assert.Throws<InventoryDomainException>(action);
    }

    [Fact]
    public void AdjustValue_ShouldRecalculateAverageCost()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            20,
            new InventoryCost(50));

        inventory.AdjustValue(-40);

        Assert.Equal(20, inventory.QuantityOnHand);
        Assert.Equal(960, inventory.InventoryValue);
        Assert.Equal(48, inventory.AverageUnitCost.Value);
    }
}
