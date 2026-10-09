using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.VoidSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.VoidSale;

public class VoidSaleHandlerTests
{
    [Fact]
    public async Task Handle_VoidsSaleAndPersistsAudit()
    {
        var userId = Guid.NewGuid();
        var sale = CreateCompletedSale();
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(
                sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new VoidSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await handler.Handle(
            new VoidSaleCommand(sale.Id, "Duplicate transaction"),
            CancellationToken.None);

        Assert.Equal(SaleStatus.Voided, sale.Status);
        Assert.Equal(userId, sale.VoidedByUserId);
        Assert.Equal("Duplicate transaction", sale.VoidReason);
        Assert.NotNull(sale.VoidedAt);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsUnauthenticatedUserWithoutSaving()
    {
        var sale = CreateCompletedSale();
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(
                sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
        currentUser.SetupGet(x => x.UserId).Returns((Guid?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new VoidSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(
            new VoidSaleCommand(sale.Id, "Duplicate transaction"),
            CancellationToken.None));

        Assert.Equal(SaleStatus.Completed, sale.Status);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundWhenSaleDoesNotExist()
    {
        var saleId = Guid.NewGuid();
        var sales = new Mock<ISaleRepository>();
        sales.Setup(x => x.GetByIdWithLinesAsync(
                saleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new VoidSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new VoidSaleCommand(saleId, "Duplicate transaction"),
            CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Sale CreateCompletedSale()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(
            sale.Id,
            Guid.NewGuid(),
            quantity: 1,
            unitSellingPrice: new Money(100m)));
        sale.Confirm();
        sale.Complete();
        return sale;
    }
}
