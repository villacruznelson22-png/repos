using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(User user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
    DateTime GetRefreshTokenExpiryUtc();
}