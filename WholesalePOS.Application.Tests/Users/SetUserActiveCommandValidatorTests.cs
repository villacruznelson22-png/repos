using WholesalePOS.Application.Users.Commands.SetUserActive;

namespace WholesalePOS.Application.Tests.Users;

public sealed class SetUserActiveCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyId_IsInvalid()
    {
        var result = new SetUserActiveCommandValidator().Validate(new SetUserActiveCommand(Guid.Empty, false));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = new SetUserActiveCommandValidator().Validate(new SetUserActiveCommand(Guid.NewGuid(), false));
        Assert.True(result.IsValid);
    }
}