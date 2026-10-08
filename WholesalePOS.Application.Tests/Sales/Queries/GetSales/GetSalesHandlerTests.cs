using Moq;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Queries.GetSales;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Tests.Sales.Queries.GetSales;

public class GetSalesHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateToSaleRepository()
    {
        var repositoryMock = new Mock<ISaleRepository>();
        var query = new GetSalesQuery
        {
            PageNumber = 2,
            PageSize = 10,
            Status = SaleStatus.Completed,
            OpenOnly = false,
            Search = "SALE-001"
        };

        var expected = new PagedResult<SaleListItemDto>
        {
            Items =
            [
                new SaleListItemDto
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = "SALE-001",
                    Status = SaleStatus.Completed,
                    TotalAmount = 500
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

        var handler = new GetSalesHandler(repositoryMock.Object);

        var result = await handler.Handle(
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
