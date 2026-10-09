using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.RefundSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.RefundSale;

public class RefundSaleHandlerTests
{
    [Fact]
    public async Task Handle_RecordsRefundAndPersists()
    {
        var userId = Guid.NewGuid();
        var sale = CreateCompletedSale(100m);
        var sales = CreateRepository(sale);
        var currentUser = CreateAuthenticatedUser(userId);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RefundSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        var refundId = await handler.Handle(
            new RefundSaleCommand(
                sale.Id, 30m, PaymentMethod.Cash, "Customer return", "refund-001"),
            CancellationToken.None);

        var refund = Assert.Single(sale.Refunds);
        Assert.Equal(refundId, refund.Id);
        Assert.Equal(30m, refund.Amount.Value);
        Assert.Equal(userId, refund.RefundedByUserId);
        Assert.Equal("Customer return", refund.Reason);
        Assert.Equal(70m, sale.GetRefundedAmount() is var total ? 100m - total : 0m);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsRefundGreaterThanRemainingPaidAmount()
    {
        var sale = CreateCompletedSale(100m);
        var sales = CreateRepository(sale);
        var currentUser = CreateAuthenticatedUser(Guid.NewGuid());
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RefundSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await handler.Handle(
            new RefundSaleCommand(
                sale.Id, 70m, PaymentMethod.Cash, "First refund", "refund-001"),
            CancellationToken.None);

        await Assert.ThrowsAsync<SaleDomainException>(() => handler.Handle(
            new RefundSaleCommand(
                sale.Id, 40m, PaymentMethod.Cash, "Too much", "refund-002"),
            CancellationToken.None));

        Assert.Single(sale.Refunds);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsExistingRefundForSameIdempotentRequest()
    {
        var sale = CreateCompletedSale(100m);
        var existing = new SaleRefund(
            sale.Id, new Money(25m), PaymentMethod.GCash, DateTime.UtcNow,
            Guid.NewGuid(), "Refund", "retry-key");
        sale.AddRefund(existing);

        var sales = CreateRepository(sale);
        sales.Setup(x => x.GetRefundByIdempotencyKeyAsync(
                "retry-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var currentUser = CreateAuthenticatedUser(Guid.NewGuid());
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RefundSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        var result = await handler.Handle(
            new RefundSaleCommand(
                sale.Id, 25m, PaymentMethod.GCash, "Refund", "retry-key"),
            CancellationToken.None);

        Assert.Equal(existing.Id, result);
        Assert.Single(sale.Refunds);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsIdempotencyKeyReusedWithDifferentPayload()
    {
        var sale = CreateCompletedSale(100m);
        var existing = new SaleRefund(
            sale.Id, new Money(25m), PaymentMethod.GCash, DateTime.UtcNow,
            Guid.NewGuid(), "Refund", "retry-key");
        var sales = CreateRepository(sale);
        sales.Setup(x => x.GetRefundByIdempotencyKeyAsync(
                "retry-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var currentUser = CreateAuthenticatedUser(Guid.NewGuid());
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RefundSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<SaleDomainException>(() => handler.Handle(
            new RefundSaleCommand(
                sale.Id, 35m, PaymentMethod.GCash, "Refund", "retry-key"),
            CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsUnauthenticatedUser()
    {
        var sale = CreateCompletedSale(100m);
        var sales = CreateRepository(sale);
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
        currentUser.SetupGet(x => x.UserId).Returns((Guid?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new RefundSaleHandler(
            sales.Object, currentUser.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(
            new RefundSaleCommand(
                sale.Id, 10m, PaymentMethod.Cash, "Refund", "refund-001"),
            CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<ISaleRepository> CreateRepository(Sale sale)
    {
        var repository = new Mock<ISaleRepository>();
        repository.Setup(x => x.GetByIdWithLinesAsync(
                sale.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        repository.Setup(x => x.GetRefundByIdempotencyKeyAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SaleRefund?)null);
        return repository;
    }

    private static Mock<ICurrentUser> CreateAuthenticatedUser(Guid userId)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        return currentUser;
    }

    private static Sale CreateCompletedSale(decimal amountPaid)
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(
            sale.Id, Guid.NewGuid(), 1, new Money(amountPaid)));
        sale.Confirm();
        sale.AddPayment(new Payment(
            sale.Id, PaymentMethod.Cash, new Money(amountPaid),
            DateTime.UtcNow, "payment-key"));
        sale.Complete();
        return sale;
    }
}
