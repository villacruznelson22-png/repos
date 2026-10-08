using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private const int AccessTokenLifetimeMinutes = 30;

    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
        => _configuration = configuration;

    public AccessTokenResult CreateAccessToken(User user)
    {
        var issuer = Required("Issuer");
        var audience = Required("Audience");
        var key = Required("Key");
        var expires = DateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(
            user.Roles
                .Select(role => new Claim(ClaimTypes.Role, role.Name)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires);
    }

    public string CreateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }

    public DateTime GetRefreshTokenExpiryUtc() =>
        DateTime.UtcNow.AddDays(7);

    private string Required(string name) =>
        _configuration[$"Jwt:{name}"]
        ?? throw new InvalidOperationException(
            $"Jwt:{name} configuration is required.");
}
