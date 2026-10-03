using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Inventory;

public class InventoryTransactionTests
{
    [Fact]
    public void Purchase_ShouldRequireIncreaseDirection()
    {
        var action = () => new InventoryTransaction(
            Guid.NewGuid(),
            InventoryTransactionType.Purchase,
            InventoryTransactionDirection.Decrease,
            new InventoryTransactionQuantity(10),
            new InventoryCost(65));

        Assert.Throws<InventoryDomainException>(action);
    }

    [Fact]
    public void Sale_ShouldRequireDecreaseDirection()
    {
        var action = () => new InventoryTransaction(
            Guid.NewGuid(),
            InventoryTransactionType.Sale,
            InventoryTransactionDirection.Increase,
            new InventoryTransactionQuantity(10),
            new InventoryCost(65));

        Assert.Throws<InventoryDomainException>(action);
    }

    [Fact]
    public void Purchase_ShouldCalculateTotalCost()
    {
        var transaction = new InventoryTransaction(
            Guid.NewGuid(),
            InventoryTransactionType.Purchase,
            InventoryTransactionDirection.Increase,
            new InventoryTransactionQuantity(10),
            new InventoryCost(65));

        Assert.Equal(650, transaction.TotalCost);
        Assert.Equal(10, transaction.SignedQuantity);
        Assert.Equal(650, transaction.SignedCost);
    }

    [Fact]
    public void Sale_ShouldHaveNegativeSignedValues()
    {
        var transaction = new InventoryTransaction(
            Guid.NewGuid(),
            InventoryTransactionType.Sale,
            InventoryTransactionDirection.Decrease,
            new InventoryTransactionQuantity(10),
            new InventoryCost(65));

        Assert.Equal(-10, transaction.SignedQuantity);
        Assert.Equal(-650, transaction.SignedCost);
    }

    [Fact]
    public void CostCorrection_ShouldBeValueOnly()
    {
        var referenceId = Guid.NewGuid();

        var transaction = new InventoryTransaction(
            Guid.NewGuid(),
            -150,
            "CreditMemo",
            referenceId);

        Assert.Equal(
            InventoryTransactionType.CostCorrection,
            transaction.Type);
        Assert.Equal(
            InventoryTransactionDirection.Decrease,
            transaction.Direction);
        Assert.Null(transaction.Quantity);
        Assert.Null(transaction.UnitCost);
        Assert.Equal(150, transaction.TotalCost);
        Assert.Equal(-150, transaction.SignedCost);
        Assert.Equal("CreditMemo", transaction.ReferenceType);
        Assert.Equal(referenceId, transaction.ReferenceId);
    }
}
