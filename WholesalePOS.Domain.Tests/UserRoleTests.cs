using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Domain.Tests;

public class UserRoleTests
{
    [Fact]
    public void AssignRole_ShouldAddRole()
    {
        var user = new User("nelson", "Nelson", "hash");
        var role = new Role("Admin");

        user.AssignRole(role);

        Assert.Single(user.Roles);
        Assert.True(user.HasRole("admin"));
    }

    [Fact]
    public void AssignRole_ShouldNotAddDuplicateRole()
    {
        var user = new User("nelson", "Nelson", "hash");
        var role = new Role("Admin");

        user.AssignRole(role);
        user.AssignRole(role);

        Assert.Single(user.Roles);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveMatchingRole()
    {
        var user = new User("nelson", "Nelson", "hash");
        var role = new Role("Admin");

        user.AssignRole(role);
        user.RemoveRole(role.Id);

        Assert.Empty(user.Roles);
        Assert.False(user.HasRole("Admin"));
    }

    [Fact]
    public void HasAnyRole_ShouldReturnTrueWhenAnyRoleMatches()
    {
        var user = new User("nelson", "Nelson", "hash");
        user.AssignRole(new Role("Cashier"));

        Assert.True(user.HasAnyRole("Admin", "Cashier"));
    }
}
