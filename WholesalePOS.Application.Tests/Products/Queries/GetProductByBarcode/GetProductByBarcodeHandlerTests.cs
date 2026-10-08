using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Products.Queries.GetProductByBarcode;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Products.Queries.GetProductByBarcode;

public class GetProductByBarcodeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductDto_WhenBarcodeExists()
    {
        var repositoryMock = new Mock<IProductRepository>();

        var product = new Product(
            "Coca Cola 1.5L",
            new Barcode("1234567890123"),
            new Money(80),
            new Money(75));

        repositoryMock
            .Setup(x => x.GetByBarcodeAsync(
                "1234567890123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new GetProductByBarcodeHandler(
            repositoryMock.Object);

        var result = await handler.Handle(
            new GetProductByBarcodeQuery("1234567890123"),
            CancellationToken.None);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Coca Cola 1.5L", result.Name);
        Assert.Equal("1234567890123", result.Barcode);
        Assert.Equal(75, result.SellingPrice);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenBarcodeDoesNotExist()
    {
        var repositoryMock = new Mock<IProductRepository>();

        repositoryMock
            .Setup(x => x.GetByBarcodeAsync(
                "9999999999999",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new GetProductByBarcodeHandler(
            repositoryMock.Object);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new GetProductByBarcodeQuery("9999999999999"),
                CancellationToken.None));

        Assert.Contains("9999999999999", exception.Message);
    }
}
