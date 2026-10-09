using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserHandler(IUserRepository users, IPasswordService passwords, IUnitOfWork unitOfWork)
    { _users = users; _passwords = passwords; _unitOfWork = unitOfWork; }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (await _users.GetByUsernameAsync(username, cancellationToken) is not null)
            throw UserErrors.UsernameAlreadyExists(username);

        var roleNames = request.Roles.Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var roles = new List<Role>();
        foreach (var roleName in roleNames)
        {
            var role = await _users.GetRoleByNameAsync(roleName, cancellationToken)
                ?? throw UserErrors.RoleNotFound(roleName);
            roles.Add(role);
        }

        var user = new User(username, request.DisplayName, _passwords.Hash(request.Password));
        foreach (var role in roles) user.AssignRole(role);
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}