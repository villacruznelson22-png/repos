using Moq;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Sales.Commands.ApplySaleDiscount;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.ApplySaleDiscount;

public class ApplySaleDiscountHandlerTests
{
    [Fact]
    public async Task Handle_AppliesDiscountAndSaves()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(sale.Id, Guid.NewGuid(), 2, new Money(100m)));
        var userId = Guid.NewGuid();
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new ApplySaleDiscountHandler(sales.Object, currentUser.Object, unitOfWork.Object);

        await handler.Handle(new ApplySaleDiscountCommand(
            sale.Id, null, DiscountScope.Sale, DiscountCalculationType.Percentage, 10m, "Promotion"),
            CancellationToken.None);

        Assert.Equal(20m, sale.GetDiscountTotalAmount().Value);
        Assert.Equal(180m, sale.GetTotalAmount().Value);
        Assert.Equal(userId, Assert.Single(sale.Discounts).AppliedByUserId);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsUnauthenticatedUser()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(sale.Id, Guid.NewGuid(), 1, new Money(100m)));
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
        currentUser.SetupGet(x => x.UserId).Returns((Guid?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new ApplySaleDiscountHandler(sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(
            new ApplySaleDiscountCommand(sale.Id, null, DiscountScope.Sale,
                DiscountCalculationType.FixedAmount, 10m, "Promotion"), CancellationToken.None));

        Assert.Empty(sale.Discounts);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsDiscountAboveEligibleAmount()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(sale.Id, Guid.NewGuid(), 1, new Money(100m)));
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new ApplySaleDiscountHandler(sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<WholesalePOS.Domain.Exceptions.SaleDomainException>(() => handler.Handle(
            new ApplySaleDiscountCommand(sale.Id, null, DiscountScope.Sale,
                DiscountCalculationType.FixedAmount, 101m, "Too much"), CancellationToken.None));

        Assert.Empty(sale.Discounts);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
