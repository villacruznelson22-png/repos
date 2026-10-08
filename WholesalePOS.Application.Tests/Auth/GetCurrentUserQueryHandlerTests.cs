using Moq;
using WholesalePOS.Application.Auth.Queries.GetCurrentUser;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Auth;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAuthenticatedUser()
    {
        var user = new User("nelson", "Nelson Villacruz", "hash");

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(user.Id);

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(
            currentUser.Object, users.Object);

        var result = await handler.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal("nelson", result.Username);
        Assert.Equal("Nelson Villacruz", result.DisplayName);
    }
}