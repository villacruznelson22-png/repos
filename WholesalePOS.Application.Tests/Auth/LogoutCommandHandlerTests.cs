using Moq;
using WholesalePOS.Application.Auth.Commands.Logout;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1));

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetRefreshTokenAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(x => x.HashRefreshToken("refresh")).Returns("hash");

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new LogoutCommandHandler(users.Object, tokens.Object, unitOfWork.Object);

        await handler.Handle(new LogoutCommand("refresh"), CancellationToken.None);

        Assert.NotNull(token.RevokedAt);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}