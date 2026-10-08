using Moq;
using WholesalePOS.Application.Customers.Queries.GetCustomers;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Tests.Customers.Queries.GetCustomers;

public class GetCustomersHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateToRepository()
    {
        var repositoryMock =
            new Mock<ICustomerRepository>();

        var query = new GetCustomersQuery
        {
            Search = "Juan",
            ActiveOnly = true,
            PageNumber = 2,
            PageSize = 10
        };

        var expected =
            new WholesalePOS.Application.Common.Models.PagedResult<CustomerListItemDto>
            {
                Items =
                [
                    new CustomerListItemDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Juan Dela Cruz",
                        ContactNumber = "09171234567",
                        IsActive = true
                    }
                ],
                PageNumber = 2,
                PageSize = 10,
                TotalCount = 11
            };

        repositoryMock
            .Setup(x => x.GetPagedAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler =
            new GetCustomersHandler(
                repositoryMock.Object);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.Same(expected, result);

        repositoryMock.Verify(
            x => x.GetPagedAsync(
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
