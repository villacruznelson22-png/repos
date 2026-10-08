using MediatR;
using WholesalePOS.Application.Auth.DTOs;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordService passwords,
        ITokenService tokens,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var user = await _users.GetByUsernameAsync(
            username,
            cancellationToken);

        var passwordValid = _passwords.Verify(
            request.Password,
            user?.PasswordHash ?? string.Empty);

        // Keep login failures generic so account existence and inactive status
        // are not disclosed to an unauthenticated caller.
        if (user is null || !passwordValid || !user.IsActive)
            throw AuthenticationErrors.InvalidCredentials();

        user.RecordLogin();

        var refreshToken = _tokens.CreateRefreshToken();
        var refreshTokenEntity = new RefreshToken(
            user.Id,
            _tokens.HashRefreshToken(refreshToken),
            _tokens.GetRefreshTokenExpiryUtc());

        await _users.AddRefreshTokenAsync(
            refreshTokenEntity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokens.CreateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshTokenExpiresAtUtc = refreshTokenEntity.ExpiresAt,
            User = new UserSummaryDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            }
        };
    }
}