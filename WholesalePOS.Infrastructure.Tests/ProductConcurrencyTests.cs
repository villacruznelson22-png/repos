using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests;

public class ProductConcurrencyTests
{
    [Fact]
    public async Task ShouldDetectConcurrencyConflict()
    {
        // Arrange

        var connectionString = "Server=localhost\\SQLEXPRESS;Database=WholesalePOSDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var factory = new TestDbContextFactory(connectionString);
        await using var contextA = factory.Create();
        await using var contextB = factory.Create();

        // Use an existing product in your TEST database
        var productId = Guid.Parse("B434A27A-EDDD-4C0D-925F-F5886CF1C915");

        var productA =
            await contextA.Products
                .SingleAsync(x => x.Id == productId);

        var productB =
            await contextB.Products
                .SingleAsync(x => x.Id == productId);

        // Both contexts should have the same version
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
    public async Task ShouldThrowConcurrencyException_WhenTwoContextsUpdateSameProduct()
    {
        // Arrange
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=WholesalePOSDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var factory =
            new TestDbContextFactory(connectionString);

        await using var contextA = factory.Create();
        await using var contextB = factory.Create();

        var productId =
            Guid.Parse("B434A27A-EDDD-4C0D-925F-F5886CF1C915");

        var productA = await contextA.Products
            .SingleAsync(x => x.Id == productId);

        var productB = await contextB.Products
            .SingleAsync(x => x.Id == productId);

        // Both contexts loaded the same version
        Assert.True(
            productA.Version.SequenceEqual(productB.Version));

        // Act
        var currentPrice = productA.DefaultSellingPrice.Value;

        // User A changes the product
        productA.ChangeDefaultSellingPrice(
            new Money(currentPrice + 1));

        var unitOfWorkA =
            new UnitOfWork(contextA);

        await unitOfWorkA.SaveChangesAsync(
            CancellationToken.None);

        // User B still has the old Version
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