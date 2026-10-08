using WholesalePOS.Application.Products.Queries.GetProducts;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests;

public class ProductRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldSearchByProductName()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var coke = CreateProduct("Coca Cola 1.5L", 75);
        var sprite = CreateProduct("Sprite 1.5L", 70);

        context.Products.AddRange(coke, sprite);

        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetPagedAsync(
            new GetProductsQuery
            {
                Search = "coca",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(coke.Id, item.Id);
        Assert.Equal("Coca Cola 1.5L", item.Name);
        Assert.Equal(75, item.SellingPrice);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldFilterActiveProducts()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var active = CreateProduct("Active Product", 100);
        var inactive = CreateProduct("Inactive Product", 200);
        inactive.Deactivate();

        context.Products.AddRange(active, inactive);

        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetPagedAsync(
            new GetProductsQuery
            {
                ActiveOnly = true,
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(active.Id, item.Id);
        Assert.Equal(100, item.SellingPrice);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldApplyPaginationAndReturnTotalCount()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        for (var i = 1; i <= 5; i++)
        {
            context.Products.Add(
                CreateProduct(
                    $"Product {i:00}",
                    i * 10));
        }

        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetPagedAsync(
            new GetProductsQuery
            {
                PageNumber = 2,
                PageSize = 2
            },
            CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal("Product 03", result.Items[0].Name);
        Assert.Equal("Product 04", result.Items[1].Name);
    }


    [Fact]
    [TestDatabase]
    public async Task GetByBarcodeAsync_ShouldReturnMatchingProduct()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var product = new Product(
            "Coca Cola 1.5L",
            new Barcode("1234567890123"),
            new Money(80),
            new Money(75));

        context.Products.Add(product);

        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetByBarcodeAsync(
            "1234567890123",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Coca Cola 1.5L", result.Name);
        Assert.Equal("1234567890123", result.Barcode!.Value);
    }

    [Fact]
    [TestDatabase]
    public async Task GetByBarcodeAsync_ShouldReturnNull_WhenBarcodeDoesNotExist()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var repository = new ProductRepository(context);

        var result = await repository.GetByBarcodeAsync(
            "9999999999999",
            CancellationToken.None);

        Assert.Null(result);
    }

    private static Product CreateProduct(
        string name,
        decimal sellingPrice)
    {
        return new Product(
            name,
            null,
            new Money(sellingPrice + 10),
            new Money(sellingPrice));
    }
}
