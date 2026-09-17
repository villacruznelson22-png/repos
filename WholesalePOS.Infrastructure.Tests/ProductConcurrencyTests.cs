using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests;


public class ProductConcurrencyTests
{
    [Fact]
    [TestDatabase]
    public async Task ShouldDetectConcurrencyConflict()
    {
        // Arrange
        var factory = new TestDbContextFactory();

        await using var contextA = factory.Create();
        await using var contextB = factory.Create();

        var product =
            new Product(
                "Concurrency Test Product",
                null,
                new Money(25.00m),
                new Money(25.00m));

        await contextA.Products.AddAsync(product);
        await contextA.SaveChangesAsync();

        var productId = product.Id;

        var productA =
            await contextA.Products
                .SingleAsync(x => x.Id == productId);

        var productB =
            await contextB.Products
                .SingleAsync(x => x.Id == productId);

        // Both contexts should have the same version.
        Assert.Equal(
            productA.Version,
            productB.Version);

        // Act
        productA.ChangeDefaultSellingPrice(
            new Money(26.00m));

        await contextA.SaveChangesAsync();

        productB.ChangeDefaultSellingPrice(
            new Money(27.00m));

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());
    }

    [Fact]
    [TestDatabase]
    public async Task ShouldThrowConcurrencyException_WhenTwoContextsUpdateSameProduct()
    {
        // Arrange
        var factory = new TestDbContextFactory();

        await using var contextA = factory.Create();
        await using var contextB = factory.Create();

        var product =
            new Product(
                "Concurrency Test Product",
                null,
                new Money(25.00m),
                new Money(25.00m));

        await contextA.Products.AddAsync(product);
        await contextA.SaveChangesAsync();

        var productId = product.Id;

        var productA =
            await contextA.Products
                .SingleAsync(x => x.Id == productId);

        var productB =
            await contextB.Products
                .SingleAsync(x => x.Id == productId);

        // Both contexts loaded the same version.
        Assert.True(
            productA.Version.SequenceEqual(
                productB.Version));

        // Act
        var currentPrice =
            productA.DefaultSellingPrice.Value;

        // User A changes the product.
        productA.ChangeDefaultSellingPrice(
            new Money(currentPrice + 1));

        var unitOfWorkA =
            new UnitOfWork(contextA);

        await unitOfWorkA.SaveChangesAsync(
            CancellationToken.None);

        // User B still has the old Version.
        productB.ChangeDefaultSellingPrice(
            new Money(currentPrice + 2));

        var unitOfWorkB =
            new UnitOfWork(contextB);

        // Assert
        await Assert.ThrowsAsync<ConcurrencyException>(
            () => unitOfWorkB.SaveChangesAsync(
                CancellationToken.None));
    }
}