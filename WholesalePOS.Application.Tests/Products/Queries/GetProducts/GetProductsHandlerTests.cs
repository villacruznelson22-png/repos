using Moq;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Products.Queries.GetProducts;

namespace WholesalePOS.Application.Tests.Products.Queries.GetProducts;

public class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateToProductRepository()
    {
        var repositoryMock = new Mock<IProductRepository>();

        var query = new GetProductsQuery
        {
            PageNumber = 2,
            PageSize = 10,
            Search = "Coke",
            ActiveOnly = true
        };

        var expected = new PagedResult<ProductListItemDto>
        {
            Items =
            [
                new ProductListItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Coke",
                    SellingPrice = 50
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

        var handler = new GetProductsHandler(repositoryMock.Object);

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
