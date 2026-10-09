using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.Commands.ChangeUserPassword;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Users;

public sealed class ChangeUserPasswordHandlerTests
{
    [Fact]
    public async Task Handle_ExistingUser_HashesPasswordAndRevokesRefreshTokens()
    {
        var users = new Mock<IUserRepository>();
        var passwords = new Mock<IPasswordService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var user = new User("cashier", "Cashier", "old-hash");
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        users.Setup(x => x.RevokeAllRefreshTokensForUserAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        passwords.Setup(x => x.Hash("StrongPassword123")).Returns("new-hash");
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new ChangeUserPasswordHandler(users.Object, passwords.Object, unitOfWork.Object);
        await handler.Handle(new ChangeUserPasswordCommand(user.Id, "StrongPassword123"), CancellationToken.None);

        Assert.Equal("new-hash", user.PasswordHash);
        users.Verify(x => x.RevokeAllRefreshTokensForUserAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingUser_ThrowsNotFound()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = new ChangeUserPasswordHandler(users.Object, Mock.Of<IPasswordService>(), Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ChangeUserPasswordCommand(Guid.NewGuid(), "StrongPassword123"), CancellationToken.None));
    }
}