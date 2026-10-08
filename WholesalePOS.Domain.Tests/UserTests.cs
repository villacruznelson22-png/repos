using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveUser()
    {
        var user = new User("nelson", "Nelson Villacruz", "hashed-password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("nelson", user.Username);
        Assert.Equal("Nelson Villacruz", user.DisplayName);
        Assert.True(user.IsActive);
        Assert.Equal("hashed-password", user.PasswordHash);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyUsername()
    {
        Assert.Throws<UserDomainException>(
            () => new User("", "Nelson", "hash"));
    }

    [Fact]
    public void Deactivate_ShouldMakeUserInactive()
    {
        var user = new User("nelson", "Nelson", "hash");

        user.Deactivate();

        Assert.False(user.IsActive);
    }
}