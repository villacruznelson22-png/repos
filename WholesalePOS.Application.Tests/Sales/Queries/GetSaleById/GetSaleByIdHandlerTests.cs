using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Queries.GetSaleById;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Queries.GetSaleById;

public class GetSaleByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSaleWithLinesAndPayments()
    {
        var saleRepositoryMock = new Mock<ISaleRepository>();

        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(75),
            new Money(70));

        var sale = new Sale(
            customerId: null,
            occurredAt: DateTime.UtcNow,
            referenceNumber: "SALE-001",
            notes: "Test sale");

        var line = new SaleLine(
            sale.Id,
            product.Id,
            2,
            new Money(70));

        typeof(SaleLine)
            .GetProperty(nameof(SaleLine.Product))!
            .SetValue(line, product);

        sale.AddLine(line);

        sale.Confirm();

        sale.AddPayment(
            new Payment(
                sale.Id,
                PaymentMethod.Cash,
                new Money(140),
                DateTime.UtcNow,
                "payment-001"));

        saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var handler = new GetSaleByIdHandler(
            saleRepositoryMock.Object);

        var result = await handler.Handle(
            new GetSaleByIdQuery(sale.Id),
            CancellationToken.None);

        Assert.Equal(sale.Id, result.Id);
        Assert.Equal("SALE-001", result.ReferenceNumber);
        Assert.Equal(SaleStatus.Confirmed, result.Status);
        Assert.Equal(140, result.TotalAmount);

        var line = Assert.Single(result.Lines);
        Assert.Equal(product.Id, line.ProductId);
        Assert.Equal("Coca-Cola 1.5L", line.ProductName);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(70, line.UnitSellingPrice);
        Assert.Equal(140, line.LineTotal);

        var payment = Assert.Single(result.Payments);
        Assert.Equal((int)PaymentMethod.Cash, payment.Method);
        Assert.Equal(140, payment.Amount);
        Assert.Equal("payment-001", payment.ReferenceNumber);
    }

    [Fact]
    public async Task Handle_WhenSaleDoesNotExist_ShouldThrowNotFound()
    {
        var saleRepositoryMock = new Mock<ISaleRepository>();

        var saleId = Guid.NewGuid();

        saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                saleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        var handler = new GetSaleByIdHandler(
            saleRepositoryMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new GetSaleByIdQuery(saleId),
                CancellationToken.None));
    }
}
