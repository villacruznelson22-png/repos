using MediatR;
using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserSummaryDto>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _users;

    public GetCurrentUserQueryHandler(ICurrentUser currentUser, IUserRepository users)
    {
        _currentUser = currentUser; _users = users;
    }

    public async Task<UserSummaryDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) throw AuthenticationErrors.NotAuthenticated();
        var user = await _users.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (user is null || !user.IsActive) throw AuthenticationErrors.NotAuthenticated();
        return new UserSummaryDto { Id=user.Id, Username=user.Username, DisplayName=user.DisplayName };
    }
}