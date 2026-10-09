using Moq;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.DTOs;
using WholesalePOS.Application.Users.Queries.GetUsers;

namespace WholesalePOS.Application.Tests.Users;

public sealed class GetUsersHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesFilteringAndPagingToRepository()
    {
        var users = new Mock<IUserRepository>();
        var query = new GetUsersQuery { Search = "cash", IsActive = true, PageNumber = 2, PageSize = 10 };
        var expected = new PagedResult<UserListItemDto> { PageNumber = 2, PageSize = 10, TotalCount = 11 };
        users.Setup(x => x.GetPagedAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var handler = new GetUsersHandler(users.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Same(expected, result);
        users.Verify(x => x.GetPagedAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}