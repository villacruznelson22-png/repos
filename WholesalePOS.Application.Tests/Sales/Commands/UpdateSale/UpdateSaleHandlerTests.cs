using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.UpdateSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.UpdateSale;

public class UpdateSaleHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSaleAndLines()
    {
        var saleRepositoryMock = new Mock<ISaleRepository>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var productA = CreateProduct("Coca-Cola", 70);
        var productB = CreateProduct("Sprite", 65);

        var sale = new Sale(
            null,
            DateTime.UtcNow,
            "SALE-001",
            "Original");

        sale.AddLine(
            new SaleLine(
                sale.Id,
                productA.Id,
                2,
                new Money(70)));

        sale.Confirm();

        customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer("Customer"));

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                id == productA.Id ? productA : productB);

        saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var handler = new UpdateSaleHandler(
            customerRepositoryMock.Object,
            productRepositoryMock.Object,
            saleRepositoryMock.Object,
            unitOfWorkMock.Object);

        await handler.Handle(
            new UpdateSaleCommand(
                sale.Id,
                null,
                DateTime.UtcNow.AddMinutes(5),
                "SALE-002",
                "Updated",
                new[]
                {
                    new UpdateSaleLineRequest(
                        productA.Id,
                        3,
                        72),
                    new UpdateSaleLineRequest(
                        productB.Id,
                        1,
                        65)
                }),
            CancellationToken.None);

        Assert.Equal("SALE-002", sale.ReferenceNumber);
        Assert.Equal("Updated", sale.Notes);
        Assert.Equal(2, sale.Lines.Count);

        var updatedLine = sale.Lines.Single(x => x.ProductId == productA.Id);
        Assert.Equal(3, updatedLine.Quantity);
        Assert.Equal(72, updatedLine.UnitSellingPrice.Value);

        Assert.Contains(
            sale.Lines,
            x => x.ProductId == productB.Id &&
                 x.Quantity == 1);

        unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRemoveLineNotIncludedInRequest()
    {
        var saleRepositoryMock = new Mock<ISaleRepository>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var productA = CreateProduct("Coca-Cola", 70);
        var productB = CreateProduct("Sprite", 65);

        var sale = new Sale(
            null,
            DateTime.UtcNow,
            "SALE-001");

        sale.AddLine(new SaleLine(
            sale.Id,
            productA.Id,
            2,
            new Money(70)));

        sale.AddLine(new SaleLine(
            sale.Id,
            productB.Id,
            1,
            new Money(65)));

        sale.Confirm();

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                id == productA.Id ? productA : productB);

        saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var handler = new UpdateSaleHandler(
            customerRepositoryMock.Object,
            productRepositoryMock.Object,
            saleRepositoryMock.Object,
            unitOfWorkMock.Object);

        await handler.Handle(
            new UpdateSaleCommand(
                sale.Id,
                null,
                sale.OccurredAt,
                "SALE-001",
                null,
                new[]
                {
                    new UpdateSaleLineRequest(
                        productA.Id,
                        5,
                        70)
                }),
            CancellationToken.None);

        var remaining = Assert.Single(sale.Lines);
        Assert.Equal(productA.Id, remaining.ProductId);
        Assert.Equal(5, remaining.Quantity);
    }

    [Fact]
    public async Task Handle_WhenSaleDoesNotExist_ShouldThrowNotFound()
    {
        var saleRepositoryMock = new Mock<ISaleRepository>();
        var customerRepositoryMock = new Mock<ICustomerRepository>();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var saleId = Guid.NewGuid();

        saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                saleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        var handler = new UpdateSaleHandler(
            customerRepositoryMock.Object,
            productRepositoryMock.Object,
            saleRepositoryMock.Object,
            unitOfWorkMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new UpdateSaleCommand(
                    saleId,
                    null,
                    DateTime.UtcNow,
                    null,
                    null,
                    new[]
                    {
                        new UpdateSaleLineRequest(
                            Guid.NewGuid(),
                            1,
                            10)
                    }),
                CancellationToken.None));
    }

    private static Product CreateProduct(
        string name,
        decimal price)
    {
        return new Product(
            name,
            null,
            new Money(price + 5),
            new Money(price));
    }
}
