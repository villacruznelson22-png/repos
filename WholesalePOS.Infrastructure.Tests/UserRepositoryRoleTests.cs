using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests;

public class UserRepositoryRoleTests
{
    [Fact]
    [TestDatabase]
    public async Task GetByUsernameAsync_ShouldIncludeRoles()
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.Create();

        var user = new User("nelson", "Nelson", "hash");
        var role = new Role("Admin");

        user.AssignRole(role);

        context.Users.Add(user);
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetByUsernameAsync(
            "nelson",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Roles);
        Assert.Equal("Admin", result.Roles.Single().Name);
    }

    [Fact]
    [TestDatabase]
    public async Task GetRoleByNameAsync_ShouldReturnRole()
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.Create();

        var role = new Role("Admin");
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetRoleByNameAsync(
            "Admin",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(role.Id, result.Id);
    }
}
