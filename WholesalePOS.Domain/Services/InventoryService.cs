using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Services;

/// <summary>
/// Coordinates inventory domain operations and creates the corresponding
/// inventory ledger transaction.
/// </summary>
public class InventoryService
{
    /// <summary>
    /// Receives inventory into the current balance and creates a Purchase
    /// inventory transaction describing the event.
    /// </summary>
    public InventoryTransaction Receive(
        InventoryBalance balance,
        decimal quantity,
        InventoryCost unitCost,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        // InventoryBalance owns the MWAC calculation and inventory invariants.
        balance.Receive(quantity, unitCost);

        // The ledger records the exact cost supplied by the source document.
        return new InventoryTransaction(
            balance.ProductId,
            InventoryTransactionType.Purchase,
            InventoryTransactionDirection.Increase,
            new InventoryTransactionQuantity(quantity),
            unitCost,
            referenceType,
            referenceId);
    }

    /// <summary>
    /// Consumes inventory using the current moving-average cost and creates
    /// the corresponding Sale or Damage transaction.
    /// </summary>
    public InventoryTransaction Consume(
        InventoryBalance balance,
        decimal quantity,
        InventoryTransactionType type,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        // BUSINESS RULE: only events that physically consume inventory may
        // use this operation. This prevents accidental use for receipts,
        // returns, or value-only corrections.
        if (type is not InventoryTransactionType.Sale
            and not InventoryTransactionType.Damage)
        {
            throw new ArgumentException(
                "Consume only supports Sale or Damage transactions.",
                nameof(type));
        }

        // Consume returns the exact MWAC snapshot used by this event.
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

    /// <summary>
    /// Applies a value-only correction to inventory and creates the corresponding
    /// CostCorrection ledger transaction.
    /// </summary>
    public InventoryTransaction AdjustValue(
        InventoryBalance balance,
        decimal valueAdjustment,
        string referenceType,
        Guid referenceId)
    {
        // The balance validates the resulting inventory state and recalculates
        // MWAC when the carrying value changes.
        balance.AdjustValue(valueAdjustment);

        return new InventoryTransaction(
            balance.ProductId,
            valueAdjustment,
            referenceType,
            referenceId);
    }
}
