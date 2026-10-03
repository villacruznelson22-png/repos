using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class InventoryTransaction
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }

    public InventoryTransactionType Type { get; private set; }
    public InventoryTransactionDirection Direction { get; private set; }

    public InventoryTransactionQuantity Quantity { get; private set; } = null!;

    // Historical/current cost associated with the quantity movement.
    public InventoryCost? UnitCost { get; private set; }

    // Total inventory value affected by this transaction.
    public decimal TotalCost { get; private set; }

    public DateTime OccurredAt { get; private set; }

    // Identifies the business document that caused this transaction.
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }

    private InventoryTransaction()
    {
        // Used by EF Core
    }

    public InventoryTransaction(
        Guid productId,
        InventoryTransactionType type,
        InventoryTransactionDirection direction,
        InventoryTransactionQuantity quantity,
        InventoryCost? unitCost,
        string? referenceType = null,
        Guid? referenceId = null,
        DateTime? occurredAt = null)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException(
                "Product ID cannot be empty.");

        ValidateDirection(type, direction);

        if (unitCost is null && type != InventoryTransactionType.CostCorrection)
            throw new InventoryDomainException(
                "Unit cost is required for quantity inventory transactions.");

        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Direction = direction;
        Quantity = quantity;
        UnitCost = unitCost;
        TotalCost = unitCost is null
            ? 0
            : decimal.Round(
                quantity.Value * unitCost.Value,
                6,
                MidpointRounding.AwayFromZero);

        OccurredAt = occurredAt ?? DateTime.UtcNow;

        ReferenceType = string.IsNullOrWhiteSpace(referenceType)
            ? null
            : referenceType.Trim();

        ReferenceId = referenceId;
    }

    public InventoryTransaction(
        Guid productId,
        decimal valueAdjustment,
        string referenceType,
        Guid referenceId,
        DateTime? occurredAt = null)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException(
                "Product ID cannot be empty.");

        if (valueAdjustment == 0)
            throw new InventoryDomainException(
                "Inventory value adjustment cannot be zero.");

        if (string.IsNullOrWhiteSpace(referenceType))
            throw new InventoryDomainException(
                "Reference type is required.");

        if (referenceId == Guid.Empty)
            throw new InventoryDomainException(
                "Reference ID cannot be empty.");

        Id = Guid.NewGuid();
        ProductId = productId;
        Type = InventoryTransactionType.CostCorrection;
        Direction = valueAdjustment > 0
            ? InventoryTransactionDirection.Increase
            : InventoryTransactionDirection.Decrease;
        Quantity = new InventoryTransactionQuantity(1);
        TotalCost = decimal.Round(
            Math.Abs(valueAdjustment),
            6,
            MidpointRounding.AwayFromZero);
        OccurredAt = occurredAt ?? DateTime.UtcNow;
        ReferenceType = referenceType.Trim();
        ReferenceId = referenceId;
    }

    public decimal SignedQuantity =>
        Direction == InventoryTransactionDirection.Increase
            ? Quantity.Value
            : -Quantity.Value;

    public decimal SignedCost =>
        Direction == InventoryTransactionDirection.Increase
            ? TotalCost
            : -TotalCost;

    private static void ValidateDirection(
        InventoryTransactionType type,
        InventoryTransactionDirection direction)
    {
        switch (type)
        {
            case InventoryTransactionType.OpeningBalance:
            case InventoryTransactionType.Purchase:
            case InventoryTransactionType.CustomerReturn:
                if (direction != InventoryTransactionDirection.Increase)
                    throw new InventoryDomainException(
                        $"{type} must increase inventory.");
                break;

            case InventoryTransactionType.Sale:
            case InventoryTransactionType.Damage:
                if (direction != InventoryTransactionDirection.Decrease)
                    throw new InventoryDomainException(
                        $"{type} must decrease inventory.");
                break;

            case InventoryTransactionType.Adjustment:
                break;

            case InventoryTransactionType.CostCorrection:
                throw new InventoryDomainException(
                    "Cost correction must use the value-adjustment constructor.");

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
