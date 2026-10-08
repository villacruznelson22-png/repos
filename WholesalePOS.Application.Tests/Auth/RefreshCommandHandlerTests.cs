using Moq;
using WholesalePOS.Application.Auth.Commands.Refresh;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Auth;

public class RefreshCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRotateRefreshToken()
    {
        var user = new User("nelson", "Nelson", "hash");
        var current = new RefreshToken(
            user.Id,
            "old-hash",
            DateTime.UtcNow.AddDays(1));

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetRefreshTokenAsync(
                "old-hash",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        users.Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        users.Setup(x => x.RevokeRefreshTokenAsync(
                current.Id,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(x => x.HashRefreshToken("old-refresh"))
            .Returns("old-hash");
        tokens.Setup(x => x.CreateRefreshToken())
            .Returns("new-refresh");
        tokens.Setup(x => x.HashRefreshToken("new-refresh"))
            .Returns("new-hash");
        tokens.Setup(x => x.GetRefreshTokenExpiryUtc())
            .Returns(DateTime.UtcNow.AddDays(7));
        tokens.Setup(x => x.CreateAccessToken(user))
            .Returns("access");

        var handler = new RefreshCommandHandler(
            users.Object,
            tokens.Object,
            Mock.Of<IUnitOfWork>());

        var result = await handler.Handle(
            new RefreshCommand("old-refresh"),
            CancellationToken.None);

        Assert.Equal("access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);

        users.Verify(x => x.RevokeRefreshTokenAsync(
            current.Id,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        users.Verify(x => x.AddRefreshTokenAsync(
            It.IsAny<RefreshToken>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectRefreshTokenAlreadyConsumedConcurrently()
    {
        var user = new User("nelson", "Nelson", "hash");
        var current = new RefreshToken(
            user.Id,
            "old-hash",
            DateTime.UtcNow.AddDays(1));

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetRefreshTokenAsync(
                "old-hash",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        users.Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        users.Setup(x => x.RevokeRefreshTokenAsync(
                current.Id,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RefreshCommandHandler(
            users.Object,
            Mock.Of<ITokenService>(),
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(
                new RefreshCommand("old-refresh"),
                CancellationToken.None));
    }
}
