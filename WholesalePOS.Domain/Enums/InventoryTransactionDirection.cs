namespace WholesalePOS.Domain.Enums;

/// <summary>
/// Describes whether an inventory transaction increases or decreases
/// the inventory state.
/// </summary>
public enum InventoryTransactionDirection
{
    /// <summary>Adds quantity/value to inventory.</summary>
    Increase = 1,

    /// <summary>Removes quantity/value from inventory.</summary>
    Decrease = 2
}
