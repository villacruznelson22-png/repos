using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests.PurchaseOrders;

public class PurchaseOrderRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetByIdWithLinesAsync_ShouldLoadSupplierAndProducts()
    {
        await using var context =
            new TestDbContextFactory().Create();

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

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            product.Id,
            20,
            new Money(180));

        purchaseOrder.AddLine(line);

        context.PurchaseOrders.Add(purchaseOrder);

        await context.SaveChangesAsync();

        var repository =
            new PurchaseOrderRepository(context);

        var result =
            await repository.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                CancellationToken.None);

        Assert.NotNull(result);

        Assert.NotNull(result!.Supplier);

        Assert.Equal(
            supplier.Id,
            result.Supplier.Id);

        Assert.Equal(
            "Test Supplier",
            result.Supplier.Name);

        Assert.Single(
            result.Lines);

        var resultLine =
            result.Lines.Single();

        Assert.NotNull(
            resultLine.Product);

        Assert.Equal(
            product.Id,
            resultLine.Product.Id);

        Assert.Equal(
            "Coke Mismo",
            resultLine.Product.Name);

        Assert.Equal(
            20,
            resultLine.Quantity);

        Assert.Equal(
            180,
            resultLine.UnitCost.Value);
    }

    [Fact]
    [TestDatabase]
    public async Task GetByIdWithLinesAsync_ShouldReturnNull_WhenPurchaseOrderDoesNotExist()
    {
        await using var context =
            new TestDbContextFactory().Create();

        var repository =
            new PurchaseOrderRepository(context);

        var result =
            await repository.GetByIdWithLinesAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.Null(result);
    }
}