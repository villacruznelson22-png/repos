using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Services;

public class InventoryService
{
    public StockMovement ReceiveStock(Product product, decimal quantity)
    {
        product.AddStock(quantity);

        return new StockMovement(
            product.Id,
            StockMovementType.Purchase,
            StockMovementDirection.Increase,
            new StockMovementQuantity(quantity));
    }

    public StockMovement SellStock(Product product, decimal quantity)
    {
        product.RemoveStock(quantity);

        return new StockMovement(
            product.Id,
            StockMovementType.Sale,
            StockMovementDirection.Decrease,
            new StockMovementQuantity(quantity));
    }
}