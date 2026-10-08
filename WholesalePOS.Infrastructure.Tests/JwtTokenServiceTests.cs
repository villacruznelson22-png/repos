using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Security;

namespace WholesalePOS.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateAccessToken_ShouldIncludeUserRolesAsClaims()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "WholesalePOS",
                ["Jwt:Audience"] = "WholesalePOS.Client",
                ["Jwt:Key"] = "DevelopmentOnly_TestKey_AtLeast32CharactersLong!"
            })
            .Build();

        var user = new User("nelson", "Nelson", "hash");
        user.AssignRole(new Role("Admin"));
        user.AssignRole(new Role("Cashier"));

        var service = new JwtTokenService(configuration);

        var result = service.CreateAccessToken(user);
        var principal = new JwtSecurityTokenHandler().ValidateToken(
            result.Token,
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes("DevelopmentOnly_TestKey_AtLeast32CharactersLong!")),
                ValidateIssuer = true,
                ValidIssuer = "WholesalePOS",
                ValidateAudience = true,
                ValidAudience = "WholesalePOS.Client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(5)
            },
            out _);

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();

        Assert.Contains("Admin", roles);
        Assert.Contains("Cashier", roles);
    }
}
