namespace WholesalePOS.Domain.Enums;

/// <summary>
/// Identifies why an inventory transaction occurred.
/// Numeric values are persisted to the database, so existing values should
/// not be reordered or reused once released.
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>Initial quantity/value introduced into the system.</summary>
    OpeningBalance = 1,

    /// <summary>Inventory received from a supplier or delivery receipt.</summary>
    Purchase = 2,

    /// <summary>Inventory consumed by a customer sale.</summary>
    Sale = 3,

    /// <summary>Inventory returned by a customer.</summary>
    CustomerReturn = 4,

    /// <summary>Inventory removed because of damage or spoilage.</summary>
    Damage = 5,

    /// <summary>Manual physical quantity correction.</summary>
    Adjustment = 6,

    /// <summary>Value-only correction that does not change physical quantity.</summary>
    CostCorrection = 7
}
