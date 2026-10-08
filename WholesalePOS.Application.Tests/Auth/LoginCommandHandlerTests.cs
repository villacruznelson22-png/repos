using Moq;
using WholesalePOS.Application.Auth.Commands.Login;
using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnTokensForValidCredentials()
    {
        var user = new User("nelson", "Nelson", "hash");

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByUsernameAsync("nelson", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwords = new Mock<IPasswordService>();
        passwords.Setup(x => x.Verify("password", "hash")).Returns(true);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(x => x.CreateAccessToken(user)).Returns("access");
        tokens.Setup(x => x.CreateRefreshToken()).Returns("refresh");
        tokens.Setup(x => x.HashRefreshToken("refresh")).Returns("refresh-hash");
        tokens.Setup(x => x.GetRefreshTokenExpiryUtc()).Returns(DateTime.UtcNow.AddDays(7));

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new LoginCommandHandler(
            users.Object, passwords.Object, tokens.Object, unitOfWork.Object);

        var result = await handler.Handle(
            new LoginCommand("nelson", "password"),
            CancellationToken.None);

        Assert.Equal("access", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(user.Id, result.User.Id);
        users.Verify(x => x.AddRefreshTokenAsync(
            It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidPassword()
    {
        var user = new User("nelson", "Nelson", "hash");

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByUsernameAsync("nelson", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwords = new Mock<IPasswordService>();
        passwords.Setup(x => x.Verify("wrong", "hash")).Returns(false);

        var handler = new LoginCommandHandler(
            users.Object,
            passwords.Object,
            Mock.Of<ITokenService>(),
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<WholesalePOS.Application.Common.Exceptions.UnauthorizedException>(
            () => handler.Handle(new LoginCommand("nelson", "wrong"), CancellationToken.None));
    }
}