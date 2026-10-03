using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

/// <summary>
/// Immutable-style audit record describing a change to inventory.
/// The transaction records what happened; <see cref="InventoryBalance"/>
/// represents the resulting current state.
/// </summary>
public class InventoryTransaction
{
    /// <summary>
    /// Unique identifier of the inventory transaction.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Product affected by this inventory transaction.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Business reason/category for the inventory movement,
    /// such as Purchase, Sale, Damage, or CostCorrection.
    /// </summary>
    public InventoryTransactionType Type { get; private set; }

    /// <summary>
    /// Indicates whether the transaction increases or decreases
    /// the inventory quantity/value.
    /// </summary>
    public InventoryTransactionDirection Direction { get; private set; }

    /// <summary>
    /// Physical quantity affected by the transaction.
    /// This is null for value-only transactions such as CostCorrection.
    /// </summary>
    public InventoryTransactionQuantity? Quantity { get; private set; }

    /// <summary>
    /// Unit inventory cost associated with the quantity movement.
    /// For a Sale, this is the historical MWAC snapshot used for COGS.
    /// </summary>
    public InventoryCost? UnitCost { get; private set; }

    /// <summary>
    /// Total inventory value affected by this transaction.
    /// For quantity movements, this is Quantity × UnitCost.
    /// </summary>
    public decimal TotalCost { get; private set; }

    /// <summary>
    /// UTC timestamp representing when the inventory event occurred.
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// Optional business-document type that caused the transaction,
    /// for example "DeliveryReceipt", "Sale", or "CreditMemo".
    /// </summary>
    public string? ReferenceType { get; private set; }

    /// <summary>
    /// Optional identifier of the source business document.
    /// Together with <see cref="ReferenceType"/>, this provides traceability
    /// from the inventory ledger back to the source transaction.
    /// </summary>
    public Guid? ReferenceId { get; private set; }

    private InventoryTransaction()
    {
        // EF Core uses this constructor when materializing an existing row.
    }

    /// <summary>
    /// Creates a quantity-based inventory transaction.
    /// </summary>
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

        // BUSINESS RULE: quantity movements must carry a cost.
        // CostCorrection is excluded because it uses the value-only constructor.
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

    /// <summary>
    /// Creates a value-only inventory cost correction.
    /// Physical quantity does not change.
    /// </summary>
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

        // BUSINESS RULE: a correction of zero has no inventory effect and
        // should not be recorded as a meaningless transaction.
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

        // CostCorrection changes value only; there is no physical quantity.
        Quantity = null;
        TotalCost = decimal.Round(
            Math.Abs(valueAdjustment),
            6,
            MidpointRounding.AwayFromZero);

        OccurredAt = occurredAt ?? DateTime.UtcNow;
        ReferenceType = referenceType.Trim();
        ReferenceId = referenceId;
    }

    /// <summary>
    /// Returns the quantity using the transaction's direction.
    /// Useful for stock-card/reporting calculations.
    /// </summary>
    public decimal SignedQuantity =>
        Direction == InventoryTransactionDirection.Increase
            ? Quantity?.Value ?? 0
            : -(Quantity?.Value ?? 0);

    /// <summary>
    /// Returns the inventory value using the transaction's direction.
    /// Useful for stock-card/reporting calculations.
    /// </summary>
    public decimal SignedCost =>
        Direction == InventoryTransactionDirection.Increase
            ? TotalCost
            : -TotalCost;

    /// <summary>
    /// Enforces which directions are valid for each transaction type.
    /// </summary>
    private static void ValidateDirection(
        InventoryTransactionType type,
        InventoryTransactionDirection direction)
    {
        switch (type)
        {
            case InventoryTransactionType.OpeningBalance:
            case InventoryTransactionType.Purchase:
            case InventoryTransactionType.CustomerReturn:
                // BUSINESS RULE: these events add inventory.
                if (direction != InventoryTransactionDirection.Increase)
                    throw new InventoryDomainException(
                        $"{type} must increase inventory.");
                break;

            case InventoryTransactionType.Sale:
            case InventoryTransactionType.Damage:
                // BUSINESS RULE: these events consume inventory.
                if (direction != InventoryTransactionDirection.Decrease)
                    throw new InventoryDomainException(
                        $"{type} must decrease inventory.");
                break;

            case InventoryTransactionType.Adjustment:
                // Adjustment direction depends on the correction being made.
                break;

            case InventoryTransactionType.CostCorrection:
                // CostCorrection has its own constructor because it changes
                // value without changing physical quantity.
                throw new InventoryDomainException(
                    "Cost correction must use the value-adjustment constructor.");

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
