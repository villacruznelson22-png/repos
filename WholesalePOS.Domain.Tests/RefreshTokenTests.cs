using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveToken()
    {
        var token = new RefreshToken(
            Guid.NewGuid(),
            "hash",
            DateTime.UtcNow.AddDays(1));

        Assert.True(token.IsActive(DateTime.UtcNow));
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldMakeTokenInactive()
    {
        var token = new RefreshToken(
            Guid.NewGuid(),
            "hash",
            DateTime.UtcNow.AddDays(1));

        token.Revoke();

        Assert.NotNull(token.RevokedAt);
        Assert.False(token.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_ShouldRejectExpiredToken()
    {
        Assert.Throws<UserDomainException>(
            () => new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddMinutes(-1)));
    }
}