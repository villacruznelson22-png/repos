using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Auth.Validators;

namespace WholesalePOS.Application.Tests.Auth;

public class AuthValidatorTests
{
    [Fact]
    public void LoginValidator_ShouldRejectMissingCredentials()
    {
        var result = new LoginRequestValidator().Validate(
            new LoginRequest("", ""));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RefreshValidator_ShouldRejectEmptyToken()
    {
        var result = new RefreshRequestValidator().Validate(
            new RefreshRequest(""));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LogoutValidator_ShouldRejectEmptyToken()
    {
        var result = new LogoutRequestValidator().Validate(
            new LogoutRequest(""));

        Assert.False(result.IsValid);
    }
}