using WholesalePOS.Application.Authorization;

namespace WholesalePOS.Application.Tests.Authorization;

public class AuthorizationPolicyTests
{
    [Fact]
    public void AdminOnly_ShouldHaveExpectedName()
        => Assert.Equal("AdminOnly", AuthorizationPolicies.AdminOnly);

    [Fact]
    public void ManagerOrAdmin_ShouldHaveExpectedName()
        => Assert.Equal("ManagerOrAdmin", AuthorizationPolicies.ManagerOrAdmin);
}
