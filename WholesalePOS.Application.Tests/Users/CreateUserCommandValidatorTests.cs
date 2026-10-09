using WholesalePOS.Application.Users.Commands.CreateUser;

namespace WholesalePOS.Application.Tests.Users;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new CreateUserCommand("cashier01", "Cashier One", "StrongPassword123", ["Cashier"]));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_InvalidPassword_IsInvalid(string password)
    {
        var result = _validator.Validate(new CreateUserCommand("cashier01", "Cashier One", password, ["Cashier"]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRoles_IsInvalid()
    {
        var result = _validator.Validate(new CreateUserCommand("cashier01", "Cashier One", "StrongPassword123", []));
        Assert.False(result.IsValid);
    }
}