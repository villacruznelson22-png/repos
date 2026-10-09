using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.Commands.UpdateUser;
using WholesalePOS.Domain.Common;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Users;

public sealed class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_RemovingLastActiveAdminRole_ThrowsConflict()
    {
        var users = new Mock<IUserRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var admin = new User("admin2", "Administrator Two", "hash");
        admin.AssignRole(new Role(RoleNames.Admin));
        var cashierRole = new Role(RoleNames.Cashier);

        users.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        users.Setup(x => x.GetByUsernameAsync(admin.Username, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        users.Setup(x => x.GetRoleByNameAsync(RoleNames.Cashier, It.IsAny<CancellationToken>())).ReturnsAsync(cashierRole);
        users.Setup(x => x.CountActiveAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateUserHandler(users.Object, currentUser.Object, unitOfWork.Object);
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new UpdateUserCommand(admin.Id, admin.Username, admin.DisplayName, [RoleNames.Cashier]),
                CancellationToken.None));

        Assert.True(admin.IsActive);
        Assert.True(admin.HasRole(RoleNames.Admin));
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
