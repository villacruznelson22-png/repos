using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordHandler : IRequestHandler<ChangeUserPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserPasswordHandler(IUserRepository users, IPasswordService passwords, IUnitOfWork unitOfWork)
    { _users = users; _passwords = passwords; _unitOfWork = unitOfWork; }

    public async Task Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw UserErrors.NotFound(request.Id);
        user.SetPasswordHash(_passwords.Hash(request.NewPassword));
        await _users.RevokeAllRefreshTokensForUserAsync(user.Id, DateTime.UtcNow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}