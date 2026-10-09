using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Users.Commands.SetUserActive;

public sealed class SetUserActiveHandler : IRequestHandler<SetUserActiveCommand>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserActiveHandler(IUserRepository users, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    { _users = users; _currentUser = currentUser; _unitOfWork = unitOfWork; }

    public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw UserErrors.NotFound(request.Id);
        if (!request.IsActive && _currentUser.UserId == user.Id)
            throw UserErrors.CannotDeactivateSelf();
        if (request.IsActive)
            user.Activate();
        else
        {
            user.Deactivate();
            await _users.RevokeAllRefreshTokensForUserAsync(user.Id, DateTime.UtcNow, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}