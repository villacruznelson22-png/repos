using Moq;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.Commands.CreateUser;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Users;

public sealed class CreateUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesUserWithHashedPasswordAndRole()
    {
        var users = new Mock<IUserRepository>();
        var passwords = new Mock<IPasswordService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var role = new Role("Cashier");
        User? added = null;

        users.Setup(x => x.GetByUsernameAsync("cashier01", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        users.Setup(x => x.GetRoleByNameAsync("Cashier", It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        users.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => added = user)
            .Returns(Task.CompletedTask);
        passwords.Setup(x => x.Hash("StrongPassword123")).Returns("hashed-password");
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateUserHandler(users.Object, passwords.Object, unitOfWork.Object);
        var id = await handler.Handle(
            new CreateUserCommand(" cashier01 ", "Cashier One", "StrongPassword123", ["Cashier"]),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(id, added!.Id);
        Assert.Equal("cashier01", added.Username);
        Assert.Equal("hashed-password", added.PasswordHash);
        Assert.Contains(role, added.Roles);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingUsername_ThrowsConflict()
    {
        var users = new Mock<IUserRepository>();
        var passwords = new Mock<IPasswordService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        users.Setup(x => x.GetByUsernameAsync("cashier01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("cashier01", "Existing", "hash"));

        var handler = new CreateUserHandler(users.Object, passwords.Object, unitOfWork.Object);
        await Assert.ThrowsAsync<WholesalePOS.Application.Common.Exceptions.ConflictException>(
            () => handler.Handle(new CreateUserCommand("cashier01", "New", "StrongPassword123", ["Cashier"]), CancellationToken.None));

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}