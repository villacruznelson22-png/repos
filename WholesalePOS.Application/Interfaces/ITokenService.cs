using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(User user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
    DateTime GetRefreshTokenExpiryUtc();
}

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);