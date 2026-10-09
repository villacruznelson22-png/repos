using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class UserErrors
{
    public static NotFoundException NotFound(Guid id) => new($"User '{id}' was not found.");
    public static NotFoundException RoleNotFound(string name) => new($"Role '{name}' was not found.");
    public static ConflictException UsernameAlreadyExists(string username) => new($"Username '{username}' is already in use.");
    public static ConflictException CannotDeactivateSelf() => new("You cannot deactivate your own account.");
    public static ConflictException CannotRemoveOwnAdminRole() => new("You cannot remove your own Admin role.");
    public static ConflictException CannotRemoveLastActiveAdmin() =>
        new("You cannot deactivate or remove the Admin role from the last active administrator.");
}
