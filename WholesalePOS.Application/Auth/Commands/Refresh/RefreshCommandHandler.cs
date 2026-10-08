using MediatR;
using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Auth.Commands.Refresh;

public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshCommandHandler(
        IUserRepository users,
        ITokenService tokens,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);

        var current = await _users.GetRefreshTokenAsync(
            hash,
            cancellationToken);

        var now = DateTime.UtcNow;

        if (current is null || !current.IsActive(now))
            throw AuthenticationErrors.InvalidRefreshToken();

        var user = await _users.GetByIdAsync(
            current.UserId,
            cancellationToken);

        if (user is null)
            throw AuthenticationErrors.InvalidRefreshToken();

        if (!user.IsActive)
            throw AuthenticationErrors.InactiveUser();

        // Atomically consume the refresh token. This closes the race where
        // two concurrent refresh requests could otherwise both rotate it.
        var revoked = await _users.RevokeRefreshTokenAsync(
            current.Id,
            now,
            cancellationToken);

        if (!revoked)
            throw AuthenticationErrors.InvalidRefreshToken();

        var newRefreshToken = _tokens.CreateRefreshToken();
        var newRefreshEntity = new RefreshToken(
            user.Id,
            _tokens.HashRefreshToken(newRefreshToken),
            _tokens.GetRefreshTokenExpiryUtc());

        await _users.AddRefreshTokenAsync(
            newRefreshEntity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _tokens.CreateAccessToken(user),
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresAtUtc = newRefreshEntity.ExpiresAt,
            User = new UserSummaryDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            }
        };
    }
}