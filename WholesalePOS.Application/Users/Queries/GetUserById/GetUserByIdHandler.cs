using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.DTOs;

namespace WholesalePOS.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDetailsDto>
{
    private readonly IUserRepository _users;
    public GetUserByIdHandler(IUserRepository users) => _users = users;

    public async Task<UserDetailsDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw UserErrors.NotFound(request.Id);
        return new UserDetailsDto
        {
            Id = user.Id, Username = user.Username, DisplayName = user.DisplayName,
            IsActive = user.IsActive, CreatedAt = user.CreatedAt, LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(x => x.Name).OrderBy(x => x).ToArray()
        };
    }
}