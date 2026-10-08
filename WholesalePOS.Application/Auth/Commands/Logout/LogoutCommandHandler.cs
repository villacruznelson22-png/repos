using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IUserRepository users,
        ITokenService tokens,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);

        var refreshToken = await _users.GetRefreshTokenAsync(
            hash,
            cancellationToken);

        if (refreshToken is null)
            return;

        refreshToken.Revoke();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}