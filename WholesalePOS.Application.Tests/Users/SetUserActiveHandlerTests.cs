using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.Commands.SetUserActive;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Users;

public sealed class SetUserActiveHandlerTests
{
    [Fact]
    public async Task Handle_DeactivatingCurrentUser_ThrowsConflict()
    {
        var users = new Mock<IUserRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var user = new User("admin", "Administrator", "hash");
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        currentUser.SetupGet(x => x.UserId).Returns(user.Id);

        var handler = new SetUserActiveHandler(users.Object, currentUser.Object, unitOfWork.Object);
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new SetUserActiveCommand(user.Id, false), CancellationToken.None));

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeactivatingOtherUser_RevokesRefreshTokensAndSaves()
    {
        var users = new Mock<IUserRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var user = new User("cashier", "Cashier", "hash");
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        users.Setup(x => x.RevokeAllRefreshTokensForUserAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new SetUserActiveHandler(users.Object, currentUser.Object, unitOfWork.Object);
        await handler.Handle(new SetUserActiveCommand(user.Id, false), CancellationToken.None);

        Assert.False(user.IsActive);
        users.Verify(x => x.RevokeAllRefreshTokensForUserAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}