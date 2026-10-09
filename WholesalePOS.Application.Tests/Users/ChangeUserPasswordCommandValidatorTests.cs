using WholesalePOS.Application.Users.Commands.ChangeUserPassword;

namespace WholesalePOS.Application.Tests.Users;

public sealed class ChangeUserPasswordCommandValidatorTests
{
    [Fact]
    public void Validate_ShortPassword_IsInvalid()
    {
        var result = new ChangeUserPasswordCommandValidator().Validate(new ChangeUserPasswordCommand(Guid.NewGuid(), "short"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidPassword_IsValid()
    {
        var result = new ChangeUserPasswordCommandValidator().Validate(new ChangeUserPasswordCommand(Guid.NewGuid(), "StrongPassword123"));
        Assert.True(result.IsValid);
    }
}