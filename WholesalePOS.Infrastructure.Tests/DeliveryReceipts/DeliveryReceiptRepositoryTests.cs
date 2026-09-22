using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests.DeliveryReceipts;

public class DeliveryReceiptRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetByIdWithLinesAsync_ShouldLoadPurchaseOrderSupplierAndProducts()
    {
        await using var context = new TestDbContextFactory().Create();

        var supplier = new Supplier(
            "Test Supplier",
            "09123456789",
            "Test Address",
            "Test Notes");

        var product = new Product(
            "Coke Mismo",
            null,
            new Money(195),
            new Money(190));

        context.Suppliers.Add(supplier);
        context.Products.Add(product);

        await context.SaveChangesAsync();

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow,
            "PO-001",
            "Test purchase order");

        var purchaseOrderLine = new PurchaseOrderLine(
            purchaseOrder.Id,
            product.Id,
            20,
            new Money(180));

        purchaseOrder.AddLine(purchaseOrderLine);
        purchaseOrder.Post();

        context.PurchaseOrders.Add(purchaseOrder);

        await context.SaveChangesAsync();

        var deliveryReceipt = new DeliveryReceipt(
            purchaseOrder.Id,
            DateTime.UtcNow,
            "DR-001",
            "Test delivery");

        var deliveryReceiptLine = new DeliveryReceiptLine(
            deliveryReceipt.Id,
            product.Id,
            15,
            new Money(182),
            new DateTime(2027, 1, 31));

        deliveryReceipt.AddLine(deliveryReceiptLine);

        context.DeliveryReceipts.Add(deliveryReceipt);

        await context.SaveChangesAsync();

        var repository =
            new DeliveryReceiptRepository(context);

        var result =
            await repository.GetByIdWithLinesAsync(
                deliveryReceipt.Id,
                CancellationToken.None);

        Assert.NotNull(result);

        Assert.NotNull(result!.PurchaseOrder);

        Assert.Equal(
            purchaseOrder.Id,
            result.PurchaseOrder.Id);

        Assert.NotNull(
            result.PurchaseOrder.Supplier);

        Assert.Equal(
            supplier.Id,
            result.PurchaseOrder.Supplier.Id);

        Assert.Equal(
            "Test Supplier",
            result.PurchaseOrder.Supplier.Name);

        Assert.Single(result.Lines);

        var resultLine = result.Lines.Single();

        Assert.NotNull(resultLine.Product);

        Assert.Equal(
            product.Id,
            resultLine.Product.Id);

        Assert.Equal(
            "Coke Mismo",
            resultLine.Product.Name);

        Assert.Equal(
            15,
            resultLine.Quantity);

        Assert.Equal(
            182,
            resultLine.UnitCost.Value);

        Assert.Equal(
            new DateTime(2027, 1, 31),
            resultLine.ExpirationDate);
    }

    [Fact]
    [TestDatabase]
    public async Task GetByIdWithLinesAsync_ShouldReturnNull_WhenDeliveryReceiptDoesNotExist()
    {
        await using var context = new TestDbContextFactory().Create();

        var repository =
            new DeliveryReceiptRepository(context);

        var result =
            await repository.GetByIdWithLinesAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.Null(result);
    }
}