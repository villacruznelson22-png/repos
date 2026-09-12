using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Services;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void ReceiveStock_ShouldIncreaseProductStock()
    {
        var product = new Product(
            "Coke Kasalo",
            new Barcode("123456"),
             new Money(260),
            new Money(280)
           );

        var service = new InventoryService();

        var movement = service.ReceiveStock(
            product,
            10);

        Assert.Equal(
            new StockQuantity(10),
            product.Stock);

        Assert.Equal(
            StockMovementType.Purchase,
            movement.Type);

        Assert.Equal(
            StockMovementDirection.Increase,
            movement.Direction);

        Assert.Equal(
            new StockMovementQuantity(10),
            movement.Quantity);
    }
    [Fact]
    public void SellStock_ShouldDecreaseProductStock()
    {
        var product = new Product(
            "Coke Kasalo",
            new Barcode("123456"),
            new Money(260),
             new Money(280));

        product.AddStock(10);

        var service = new InventoryService();

        var movement = service.SellStock(
            product,
            0.5m);

        Assert.Equal(
            new StockQuantity(9.5m),
            product.Stock);

        Assert.Equal(
            StockMovementType.Sale,
            movement.Type);

        Assert.Equal(
            StockMovementDirection.Decrease,
            movement.Direction);

        Assert.Equal(
            new StockMovementQuantity(0.5m),
            movement.Quantity);
    }

    [Fact]
    public void SellStock_ShouldThrow_WhenStockIsInsufficient()
    {
        var product = new Product(
            "Coke Kasalo",
            new Barcode("123456"),
            new Money(260),
            new Money(280));

        product.AddStock(1);

        var service = new InventoryService();

        var action = () =>
            service.SellStock(product, 1.5m);

        Assert.Throws<InvalidOperationException>(
            action);
    }

}