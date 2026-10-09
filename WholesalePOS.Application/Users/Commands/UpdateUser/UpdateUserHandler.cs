using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Common;

namespace WholesalePOS.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserHandler(IUserRepository users, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    { _users = users; _currentUser = currentUser; _unitOfWork = unitOfWork; }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw UserErrors.NotFound(request.Id);
        var username = request.Username.Trim();
        var existing = await _users.GetByUsernameAsync(username, cancellationToken);
        if (existing is not null && existing.Id != user.Id)
            throw UserErrors.UsernameAlreadyExists(username);

        var roleNames = request.Roles.Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var roles = new List<WholesalePOS.Domain.Entities.Role>();
        foreach (var roleName in roleNames)
        {
            var role = await _users.GetRoleByNameAsync(roleName, cancellationToken)
                ?? throw UserErrors.RoleNotFound(roleName);
            roles.Add(role);
        }

        if (_currentUser.UserId == user.Id && user.HasRole(RoleNames.Admin) &&
            !roleNames.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase))
            throw UserErrors.CannotRemoveOwnAdminRole();

        user.ChangeUsername(username);
        user.ChangeDisplayName(request.DisplayName);
        foreach (var role in user.Roles.ToArray()) user.RemoveRole(role.Id);
        foreach (var role in roles) user.AssignRole(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}