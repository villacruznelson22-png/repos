using Moq;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.RemoveSaleDiscount;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.RemoveSaleDiscount;

public class RemoveSaleDiscountHandlerTests
{
    [Fact]
    public async Task Handle_ReversesDiscountAndPreservesAuditHistory()
    {
        var userId = Guid.NewGuid();
        var sale = new Sale(null, DateTime.UtcNow);
        var line = new SaleLine(sale.Id, Guid.NewGuid(), 1, new Money(100m));
        sale.AddLine(line);
        var discount = new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 20m,
            new Money(20m), "Promotion", Guid.NewGuid());
        sale.AddDiscount(discount);

        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RemoveSaleDiscountHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await handler.Handle(
            new RemoveSaleDiscountCommand(sale.Id, discount.Id, "Entered in error"),
            CancellationToken.None);

        Assert.Equal(100m, sale.GetTotalAmount().Value);
        Assert.True(discount.IsRemoved);
        Assert.Equal(userId, discount.RemovedByUserId);
        Assert.Equal("Entered in error", discount.RemovalReason);
        Assert.Single(sale.Discounts);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsUnauthenticatedUserWithoutSaving()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        var line = new SaleLine(sale.Id, Guid.NewGuid(), 1, new Money(100m));
        sale.AddLine(line);
        var discount = new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 20m,
            new Money(20m), "Promotion", Guid.NewGuid());
        sale.AddDiscount(discount);

        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
        currentUser.SetupGet(x => x.UserId).Returns((Guid?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RemoveSaleDiscountHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new RemoveSaleDiscountCommand(sale.Id, discount.Id, "Entered in error"),
            CancellationToken.None));

        Assert.False(discount.IsRemoved);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
