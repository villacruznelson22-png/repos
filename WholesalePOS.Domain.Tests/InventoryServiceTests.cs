using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Services;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void Receive_ShouldIncreaseBalanceAndCreatePurchaseTransaction()
    {
        var productId = Guid.NewGuid();
        var inventory = InventoryBalance.CreateEmpty(productId);
        var service = new InventoryService();

        var transaction = service.Receive(
            inventory,
            10,
            new InventoryCost(65),
            "DeliveryReceipt",
            Guid.NewGuid());

        Assert.Equal(10, inventory.QuantityOnHand);
        Assert.Equal(650, inventory.InventoryValue);
        Assert.Equal(65, inventory.AverageUnitCost.Value);

        Assert.Equal(productId, transaction.ProductId);
        Assert.Equal(
            InventoryTransactionType.Purchase,
            transaction.Type);
        Assert.Equal(
            InventoryTransactionDirection.Increase,
            transaction.Direction);
        Assert.Equal(10, transaction.Quantity!.Value);
        Assert.Equal(65, transaction.UnitCost!.Value);
        Assert.Equal(650, transaction.TotalCost);
    }

    [Fact]
    public void Consume_ShouldDecreaseBalanceAndCreateSaleTransaction()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            100,
            new InventoryCost(65));

        var service = new InventoryService();

        var transaction = service.Consume(
            inventory,
            10,
            InventoryTransactionType.Sale,
            "Sale",
            Guid.NewGuid());

        Assert.Equal(90, inventory.QuantityOnHand);
        Assert.Equal(5850, inventory.InventoryValue);

        Assert.Equal(
            InventoryTransactionType.Sale,
            transaction.Type);
        Assert.Equal(
            InventoryTransactionDirection.Decrease,
            transaction.Direction);
        Assert.Equal(10, transaction.Quantity!.Value);
        Assert.Equal(65, transaction.UnitCost!.Value);
        Assert.Equal(650, transaction.TotalCost);
    }

    [Fact]
    public void Consume_ShouldThrow_WhenStockIsInsufficient()
    {
        var inventory = InventoryBalance.CreateOpeningBalance(
            Guid.NewGuid(),
            1,
            new InventoryCost(65));

        var service = new InventoryService();

        var action = () => service.Consume(
            inventory,
            1.5m,
            InventoryTransactionType.Sale);

        Assert.Throws<WholesalePOS.Domain.Exceptions.InventoryDomainException>(
            action);
    }

    [Fact]
    public void AdjustValue_ShouldCreateCostCorrectionTransaction()
    {
        var productId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();

        var inventory = InventoryBalance.CreateOpeningBalance(
            productId,
            20,
            new InventoryCost(50));

        var service = new InventoryService();

        var transaction = service.AdjustValue(
            inventory,
            -40,
            "CreditMemo",
            referenceId);

        Assert.Equal(960, inventory.InventoryValue);
        Assert.Equal(48, inventory.AverageUnitCost.Value);

        Assert.Equal(
            InventoryTransactionType.CostCorrection,
            transaction.Type);
        Assert.Equal(
            InventoryTransactionDirection.Decrease,
            transaction.Direction);
        Assert.Null(transaction.Quantity);
        Assert.Equal(40, transaction.TotalCost);
        Assert.Equal("CreditMemo", transaction.ReferenceType);
        Assert.Equal(referenceId, transaction.ReferenceId);
    }
}
