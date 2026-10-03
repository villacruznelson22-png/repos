using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Services;

public class InventoryService
{
    public InventoryTransaction Receive(
        InventoryBalance balance,
        decimal quantity,
        InventoryCost unitCost,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        balance.Receive(quantity, unitCost);

        return new InventoryTransaction(
            balance.ProductId,
            InventoryTransactionType.Purchase,
            InventoryTransactionDirection.Increase,
            new InventoryTransactionQuantity(quantity),
            unitCost,
            referenceType,
            referenceId);
    }

    public InventoryTransaction Consume(
        InventoryBalance balance,
        decimal quantity,
        InventoryTransactionType type,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        if (type is not InventoryTransactionType.Sale
            and not InventoryTransactionType.Damage)
        {
            throw new ArgumentException(
                "Consume only supports Sale or Damage transactions.",
                nameof(type));
        }

        var unitCost = balance.Consume(quantity);

        return new InventoryTransaction(
            balance.ProductId,
            type,
            InventoryTransactionDirection.Decrease,
            new InventoryTransactionQuantity(quantity),
            unitCost,
            referenceType,
            referenceId);
    }

    public InventoryTransaction AdjustValue(
        InventoryBalance balance,
        decimal valueAdjustment,
        string referenceType,
        Guid referenceId)
    {
        balance.AdjustValue(valueAdjustment);

        return new InventoryTransaction(
            balance.ProductId,
            valueAdjustment,
            referenceType,
            referenceId);
    }
}
