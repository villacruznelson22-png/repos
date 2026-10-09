using WholesalePOS.Application.Users.Commands.UpdateUser;

namespace WholesalePOS.Application.Tests.Users;

public sealed class UpdateUserCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyId_IsInvalid()
    {
        var result = new UpdateUserCommandValidator().Validate(new UpdateUserCommand(Guid.Empty, "cashier01", "Cashier One", ["Cashier"]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = new UpdateUserCommandValidator().Validate(new UpdateUserCommand(Guid.NewGuid(), "cashier01", "Cashier One", ["Cashier"]));
        Assert.True(result.IsValid);
    }
}