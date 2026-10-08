using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Domain.Tests;

public class RoleTests
{
    [Fact]
    public void Constructor_ShouldRejectEmptyName()
    {
        Assert.Throws<ArgumentException>(() => new Role(" "));
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var role = new Role(" Admin ");

        Assert.Equal("Admin", role.Name);
    }
}
