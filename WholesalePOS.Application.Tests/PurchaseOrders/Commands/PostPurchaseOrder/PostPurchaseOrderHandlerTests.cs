using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.PostPurchaseOrder;

public class PostPurchaseOrderHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPostPurchaseOrder()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var repository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var handler =
            new PostPurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        await handler.Handle(
            new PostPurchaseOrderCommand(
                purchaseOrder.Id),
            CancellationToken.None);

        Assert.Equal(
            PurchaseOrderStatus.Posted,
            purchaseOrder.Status);

        repository.Verify(
            x => x.Update(purchaseOrder),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var purchaseOrderId = Guid.NewGuid();

        var repository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        var handler =
            new PostPurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new PostPurchaseOrderCommand(
                    purchaseOrderId),
                CancellationToken.None));

        repository.Verify(
            x => x.Update(
                It.IsAny<PurchaseOrder>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateDomainException_WhenPurchaseOrderHasNoLines()
    {
        var purchaseOrder = new PurchaseOrder(
            Guid.NewGuid(),
            DateTime.UtcNow);

        var repository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var handler =
            new PostPurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        await Assert.ThrowsAsync<PurchaseOrderDomainException>(
            () => handler.Handle(
                new PostPurchaseOrderCommand(
                    purchaseOrder.Id),
                CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static PurchaseOrder CreatePurchaseOrder()
    {
        var purchaseOrder = new PurchaseOrder(
            Guid.NewGuid(),
            DateTime.UtcNow);

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            Guid.NewGuid(),
            10,
            new WholesalePOS.Domain.ValueObjects.Money(180));

        purchaseOrder.AddLine(line);

        return purchaseOrder;
    }
}