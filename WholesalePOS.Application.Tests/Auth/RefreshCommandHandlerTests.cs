using Moq;
using WholesalePOS.Application.Auth.Commands.Refresh;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Auth;

public class RefreshCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRotateRefreshToken()
    {
        var user = new User("nelson", "Nelson", "hash");
        var current = new RefreshToken(user.Id, "old-hash", DateTime.UtcNow.AddDays(1));

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetRefreshTokenAsync("old-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(x => x.HashRefreshToken("old-refresh")).Returns("old-hash");
        tokens.Setup(x => x.CreateRefreshToken()).Returns("new-refresh");
        tokens.Setup(x => x.HashRefreshToken("new-refresh")).Returns("new-hash");
        tokens.Setup(x => x.GetRefreshTokenExpiryUtc()).Returns(DateTime.UtcNow.AddDays(7));
        tokens.Setup(x => x.CreateAccessToken(user)).Returns("access");

        var handler = new RefreshCommandHandler(
            users.Object, tokens.Object, Mock.Of<IUnitOfWork>());

        var result = await handler.Handle(
            new RefreshCommand("old-refresh"),
            CancellationToken.None);

        Assert.Equal("access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);
        Assert.NotNull(current.RevokedAt);
        users.Verify(x => x.AddRefreshTokenAsync(
            It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}