using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests;

public class UserRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetByUsernameAsync_ShouldReturnUser()
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.Create();

        var user = new User("nelson", "Nelson", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetByUsernameAsync(
            "nelson",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    [TestDatabase]
    public async Task GetRefreshTokenAsync_ShouldReturnMatchingToken()
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.Create();

        var user = new User("nelson", "Nelson", "hash");
        var token = new RefreshToken(user.Id, "refresh-hash", DateTime.UtcNow.AddDays(1));

        context.Users.Add(user);
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetRefreshTokenAsync(
            "refresh-hash",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
    }

    [Fact]
    [TestDatabase]
    public async Task RevokeRefreshTokenAsync_ShouldConsumeTokenOnlyOnce()
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.Create();

        var user = new User("nelson", "Nelson", "hash");
        var token = new RefreshToken(
            user.Id,
            "refresh-hash",
            DateTime.UtcNow.AddDays(1));

        context.Users.Add(user);
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);
        var now = DateTime.UtcNow;

        var first = await repository.RevokeRefreshTokenAsync(
            token.Id,
            now,
            CancellationToken.None);

        var second = await repository.RevokeRefreshTokenAsync(
            token.Id,
            now,
            CancellationToken.None);

        Assert.True(first);
        Assert.False(second);
    }
}
