namespace WholesalePOS.Domain.Enums;

public enum InventoryTransactionType
{
    OpeningBalance = 1,
    Purchase = 2,
    Sale = 3,
    CustomerReturn = 4,
    Damage = 5,
    Adjustment = 6,
    CostCorrection = 7
}
