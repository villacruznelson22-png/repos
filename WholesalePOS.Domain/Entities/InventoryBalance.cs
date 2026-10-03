using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class InventoryBalance
{
    private const int CostPrecision = 6;

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }

    public decimal QuantityOnHand { get; private set; }

    // Total historical cost of the inventory currently on hand.
    public decimal InventoryValue { get; private set; }

    // Current moving weighted-average unit cost.
    public InventoryCost AverageUnitCost { get; private set; } = null!;

    private InventoryBalance()
    {
        // Used by EF Core
    }

    private InventoryBalance(
        Guid productId,
        decimal quantityOnHand,
        decimal inventoryValue)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException(
                "Product ID cannot be empty.");

        if (quantityOnHand < 0)
            throw new InventoryDomainException(
                "Inventory quantity cannot be negative.");

        if (inventoryValue < 0)
            throw new InventoryDomainException(
                "Inventory value cannot be negative.");

        Id = Guid.NewGuid();
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        InventoryValue = RoundCost(inventoryValue);

        AverageUnitCost = new InventoryCost(
            quantityOnHand == 0
                ? 0
                : inventoryValue / quantityOnHand);
    }

    public static InventoryBalance CreateEmpty(Guid productId)
    {
        return new InventoryBalance(productId, 0, 0);
    }

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

        AverageUnitCost = new InventoryCost(
            newInventoryValue / newQuantity);
    }

    public InventoryCost Consume(decimal quantity)
    {
        EnsurePositiveQuantity(quantity);

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
            InventoryValue = 0;
            AverageUnitCost = new InventoryCost(0);
        }

        // Moving weighted-average cost does not change when stock is consumed.
        // Keep the existing average instead of deriving it again from a rounded
        // inventory value.
        return new InventoryCost(
            costAtCurrentAverage / quantity);
    }

    public void AdjustValue(decimal valueAdjustment)
    {
        var newInventoryValue =
            InventoryValue + valueAdjustment;

        if (newInventoryValue < 0)
            throw new InventoryDomainException(
                "Inventory value cannot become negative.");

        InventoryValue = RoundCost(newInventoryValue);

        AverageUnitCost = new InventoryCost(
            QuantityOnHand == 0
                ? 0
                : InventoryValue / QuantityOnHand);
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
