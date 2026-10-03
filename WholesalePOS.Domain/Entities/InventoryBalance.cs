using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

/// <summary>
/// Represents the current inventory state for one product.
/// This is the authoritative operational balance used by the costing engine.
/// </summary>
public class InventoryBalance
{
    private const int CostPrecision = 6;

    /// <summary>
    /// Unique identifier of this inventory balance record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Product whose inventory is represented by this balance.
    /// One product can have only one current inventory balance in this model.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Current physical quantity available in inventory.
    /// This value changes when inventory is received, consumed, or adjusted.
    /// </summary>
    public decimal QuantityOnHand { get; private set; }

    /// <summary>
    /// Total carrying value of the quantity currently on hand.
    /// Under moving weighted-average costing, this value is used together with
    /// <see cref="QuantityOnHand"/> to determine the current average cost.
    /// </summary>
    public decimal InventoryValue { get; private set; }

    /// <summary>
    /// Current moving weighted-average unit cost of the inventory.
    /// This is the cost that Sales will snapshot as the historical UnitCost
    /// when inventory is consumed.
    /// </summary>
    public InventoryCost AverageUnitCost { get; private set; } = null!;

    private InventoryBalance()
    {
        // EF Core uses this constructor when materializing an existing row.
    }

    private InventoryBalance(
        Guid productId,
        decimal quantityOnHand,
        decimal inventoryValue)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException(
                "Product ID cannot be empty.");

        // BUSINESS RULE: inventory quantity can never be negative.
        if (quantityOnHand < 0)
            throw new InventoryDomainException(
                "Inventory quantity cannot be negative.");

        // BUSINESS RULE: inventory carrying value can never be negative.
        if (inventoryValue < 0)
            throw new InventoryDomainException(
                "Inventory value cannot be negative.");

        Id = Guid.NewGuid();
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        InventoryValue = RoundCost(inventoryValue);

        // An empty inventory balance has no meaningful unit cost.
        AverageUnitCost = new InventoryCost(
            quantityOnHand == 0
                ? 0
                : inventoryValue / quantityOnHand);
    }

    /// <summary>
    /// Creates an inventory balance with zero quantity and zero value.
    /// This is useful when a product is receiving inventory for the first time.
    /// </summary>
    public static InventoryBalance CreateEmpty(Guid productId)
    {
        return new InventoryBalance(productId, 0, 0);
    }

    /// <summary>
    /// Creates the initial inventory state when the system is introduced
    /// with existing physical stock.
    /// </summary>
    public static InventoryBalance CreateOpeningBalance(
        Guid productId,
        decimal quantity,
        InventoryCost unitCost)
    {
        // BUSINESS RULE: opening quantity cannot be negative.
        if (quantity < 0)
            throw new InventoryDomainException(
                "Opening inventory quantity cannot be negative.");

        ArgumentNullException.ThrowIfNull(unitCost);

        return new InventoryBalance(
            productId,
            quantity,
            quantity * unitCost.Value);
    }

    /// <summary>
    /// Receives inventory at a known unit cost and recalculates the
    /// moving weighted-average cost.
    /// </summary>
    public void Receive(
        decimal quantity,
        InventoryCost unitCost)
    {
        // BUSINESS RULE: every receipt must contain a positive quantity.
        EnsurePositiveQuantity(quantity);

        ArgumentNullException.ThrowIfNull(unitCost);

        var incomingValue = quantity * unitCost.Value;
        var newQuantity = QuantityOnHand + quantity;
        var newInventoryValue = InventoryValue + incomingValue;

        QuantityOnHand = newQuantity;
        InventoryValue = RoundCost(newInventoryValue);

        // BUSINESS RULE: MWAC is recalculated only when inventory value/quantity
        // is increased by a receipt. This is the core perpetual-average formula.
        AverageUnitCost = new InventoryCost(
            newInventoryValue / newQuantity);
    }

    /// <summary>
    /// Removes inventory using the current moving-average cost.
    /// Returns the unit cost that must be captured by the consuming transaction.
    /// </summary>
    public InventoryCost Consume(decimal quantity)
    {
        // BUSINESS RULE: a sale/damage cannot consume zero or negative quantity.
        EnsurePositiveQuantity(quantity);

        // BUSINESS RULE: the domain, not the cashier/API, protects against
        // inventory going below zero.
        if (quantity > QuantityOnHand)
            throw new InventoryDomainException(
                "Insufficient inventory.");

        var costAtCurrentAverage =
            quantity * AverageUnitCost.Value;

        QuantityOnHand -= quantity;
        InventoryValue =
            RoundCost(InventoryValue - costAtCurrentAverage);

        if (QuantityOnHand == 0)
        {
            // Avoid tiny decimal residue when the final inventory is consumed.
            InventoryValue = 0;
            AverageUnitCost = new InventoryCost(0);
        }

        // BUSINESS RULE: consuming inventory does NOT recalculate MWAC.
        // The existing average remains the cost basis until another receipt
        // or value correction changes the inventory state.
        return new InventoryCost(
            costAtCurrentAverage / quantity);
    }

    /// <summary>
    /// Changes the carrying value without changing the physical quantity.
    /// Used for inventory cost corrections and similar value-only adjustments.
    /// </summary>
    public void AdjustValue(decimal valueAdjustment)
    {
        var newInventoryValue =
            InventoryValue + valueAdjustment;

        // BUSINESS RULE: an inventory correction cannot make carrying value
        // negative because that would create an invalid inventory state.
        if (newInventoryValue < 0)
            throw new InventoryDomainException(
                "Inventory value cannot become negative.");

        InventoryValue = RoundCost(newInventoryValue);

        // A value correction changes the current average when stock remains.
        AverageUnitCost = new InventoryCost(
            QuantityOnHand == 0
                ? 0
                : InventoryValue / QuantityOnHand);
    }

    /// <summary>
    /// Validates a quantity used by inventory-changing operations.
    /// </summary>
    private static void EnsurePositiveQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException(
                "Inventory quantity must be greater than zero.");
    }

    /// <summary>
    /// Keeps internal inventory valuation at six decimal places to avoid
    /// unnecessary rounding drift during repeated MWAC calculations.
    /// </summary>
    private static decimal RoundCost(decimal value)
    {
        return decimal.Round(
            value,
            CostPrecision,
            MidpointRounding.AwayFromZero);
    }
}
