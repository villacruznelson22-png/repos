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

        if (user is null || !_passwords.Verify(request.Password, user.PasswordHash))
            throw AuthenticationErrors.InvalidCredentials();

        if (!user.IsActive)
            throw AuthenticationErrors.InactiveUser();

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

        return new AuthResponse
        {
            AccessToken = _tokens.CreateAccessToken(user),
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
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