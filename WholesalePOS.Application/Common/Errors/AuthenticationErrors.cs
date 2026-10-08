using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class AuthenticationErrors
{
    public static UnauthorizedException InvalidCredentials()
        => new("Invalid username or password.");

    public static UnauthorizedException InactiveUser()
        => new("User account is inactive.");

    public static UnauthorizedException InvalidRefreshToken()
        => new("Refresh token is invalid or expired.");

    public static UnauthorizedException NotAuthenticated()
        => new("Authentication is required.");
}