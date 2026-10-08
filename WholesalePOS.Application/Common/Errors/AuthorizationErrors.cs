using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class AuthorizationErrors
{
    public static ForbiddenException Forbidden()
        => new("You are not authorized to perform this action.");
}
