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

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal InventoryValue { get; private set; }
    public InventoryCost AverageUnitCost { get; private set; } = null!;

    /// <summary>
    /// SQL Server rowversion used to detect concurrent inventory changes.
    /// </summary>
    public byte[] Version { get; private set; } = null!;

    private InventoryBalance() { }

    private InventoryBalance(
        Guid productId,
        decimal quantityOnHand,
        decimal inventoryValue)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException("Product ID cannot be empty.");

        if (inventoryValue == decimal.MinValue)
            throw new InventoryDomainException("Inventory value is outside the supported range.");

        Id = Guid.NewGuid();
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        InventoryValue = RoundCost(inventoryValue);
        AverageUnitCost = CreateAverageUnitCost(
            quantityOnHand,
            inventoryValue);
    }

    public static InventoryBalance CreateEmpty(Guid productId)
        => new(productId, 0, 0);

    public static InventoryBalance CreateOpeningBalance(
        Guid productId,
        decimal quantity,
        InventoryCost unitCost)
    {
        if (quantity < 0)
            throw new InventoryDomainException(
                "Opening inventory quantity cannot be negative.");

        ArgumentNullException.ThrowIfNull(unitCost);

        return new InventoryBalance(
            productId,
            quantity,
            quantity * unitCost.Value);
    }

    public void Receive(
        decimal quantity,
        InventoryCost unitCost)
    {
        EnsurePositiveQuantity(quantity);
        ArgumentNullException.ThrowIfNull(unitCost);

        var incomingValue = quantity * unitCost.Value;
        var newQuantity = QuantityOnHand + quantity;
        var newInventoryValue = InventoryValue + incomingValue;

        QuantityOnHand = newQuantity;
        InventoryValue = RoundCost(newInventoryValue);
        AverageUnitCost = CreateAverageUnitCost(
            newQuantity,
            newInventoryValue);
    }

    public InventoryCost Consume(
        decimal quantity,
        bool allowNegative = false,
        InventoryCost? fallbackUnitCost = null)
    {
        EnsurePositiveQuantity(quantity);

        if (quantity > QuantityOnHand && !allowNegative)
            throw new InventoryDomainException(
                "Insufficient inventory.");

        var unitCost = ResolveConsumptionCost(fallbackUnitCost);

        var costAtCurrentAverage =
            quantity * unitCost.Value;

        QuantityOnHand -= quantity;
        InventoryValue =
            RoundCost(InventoryValue - costAtCurrentAverage);

        AverageUnitCost = CreateAverageUnitCost(
            QuantityOnHand,
            InventoryValue,
            unitCost);

        return unitCost;
    }

    public void AdjustValue(decimal valueAdjustment)
    {
        var newInventoryValue =
            InventoryValue + valueAdjustment;

        InventoryValue = RoundCost(newInventoryValue);

        AverageUnitCost = CreateAverageUnitCost(
            QuantityOnHand,
            InventoryValue);
    }

    private InventoryCost ResolveConsumptionCost(
        InventoryCost? fallbackUnitCost)
    {
        if (AverageUnitCost.Value > 0)
            return AverageUnitCost;

        if (fallbackUnitCost is not null)
            return fallbackUnitCost;

        throw new InventoryDomainException(
            "Inventory cost is unavailable.");
    }

    private static InventoryCost CreateAverageUnitCost(
        decimal quantity,
        decimal inventoryValue,
        InventoryCost? fallbackUnitCost = null)
    {
        if (quantity == 0)
            return new InventoryCost(0);

        if (inventoryValue <= 0)
        {
            if (fallbackUnitCost is not null)
                return fallbackUnitCost;

            return new InventoryCost(0);
        }

        return new InventoryCost(
            inventoryValue / quantity);
    }

    private static void EnsurePositiveQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException(
                "Inventory quantity must be greater than zero.");
    }

    private static decimal RoundCost(decimal value)
    {
        return decimal.Round(
            value,
            CostPrecision,
            MidpointRounding.AwayFromZero);
    }
}
