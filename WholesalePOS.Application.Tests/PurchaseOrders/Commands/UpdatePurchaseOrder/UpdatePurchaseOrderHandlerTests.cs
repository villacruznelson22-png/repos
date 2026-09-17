using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.UpdatePurchaseOrder;

public class UpdatePurchaseOrderHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdatePurchaseOrder()
    {
        var originalOccurredAt =
            new DateTime(2026, 9, 17, 10, 0, 0, DateTimeKind.Utc);

        var updatedOccurredAt =
            new DateTime(2026, 9, 18, 14, 30, 0, DateTimeKind.Utc);

        var purchaseOrder = CreatePurchaseOrder(
            originalOccurredAt);

        var productId = Guid.NewGuid();

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            productId,
            10,
            new Money(180));

        purchaseOrder.AddLine(line);

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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            updatedOccurredAt,
            "PO-UPDATED",
            "Updated notes",
            [
                new UpdatePurchaseOrderLineRequest(
                    productId,
                    25,
                    185)
            ]);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal(
            updatedOccurredAt,
            purchaseOrder.OccurredAt);

        Assert.Equal(
            "PO-UPDATED",
            purchaseOrder.ReferenceNumber);

        Assert.Equal(
            "Updated notes",
            purchaseOrder.Notes);

        Assert.Single(
            purchaseOrder.Lines);

        Assert.Equal(
            25,
            purchaseOrder.Lines.Single().Quantity);

        Assert.Equal(
            185,
            purchaseOrder.Lines.Single().UnitCost.Value);

        repository.Verify(
            x => x.Update(purchaseOrder),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddNewLine()
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

        var newProductId = Guid.NewGuid();

        var handler =
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            purchaseOrder.OccurredAt,
            null,
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    newProductId,
                    15,
                    200)
            ]);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Single(
            purchaseOrder.Lines);

        var line = purchaseOrder.Lines.Single();

        Assert.Equal(
            newProductId,
            line.ProductId);

        Assert.Equal(
            15,
            line.Quantity);

        Assert.Equal(
            200,
            line.UnitCost.Value);

        repository.Verify(
            x => x.Update(purchaseOrder),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMissingLine()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var existingProductId = Guid.NewGuid();

        var existingLine = new PurchaseOrderLine(
            purchaseOrder.Id,
            existingProductId,
            10,
            new Money(180));

        purchaseOrder.AddLine(existingLine);

        var newProductId = Guid.NewGuid();

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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            purchaseOrder.OccurredAt,
            null,
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    newProductId,
                    20,
                    190)
            ]);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Single(
            purchaseOrder.Lines);

        Assert.Equal(
            newProductId,
            purchaseOrder.Lines.Single().ProductId);

        Assert.DoesNotContain(
            purchaseOrder.Lines,
            x => x.ProductId == existingProductId);

        repository.Verify(
            x => x.Update(purchaseOrder),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingAndAddNewLine()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var existingProductId = Guid.NewGuid();
        var newProductId = Guid.NewGuid();

        var existingLine = new PurchaseOrderLine(
            purchaseOrder.Id,
            existingProductId,
            10,
            new Money(180));

        purchaseOrder.AddLine(existingLine);

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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            purchaseOrder.OccurredAt,
            null,
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    existingProductId,
                    30,
                    185),

                new UpdatePurchaseOrderLineRequest(
                    newProductId,
                    15,
                    200)
            ]);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal(
            2,
            purchaseOrder.Lines.Count);

        var updatedLine = purchaseOrder.Lines
            .Single(x => x.ProductId == existingProductId);

        var newLine = purchaseOrder.Lines
            .Single(x => x.ProductId == newProductId);

        Assert.Equal(
            30,
            updatedLine.Quantity);

        Assert.Equal(
            185,
            updatedLine.UnitCost.Value);

        Assert.Equal(
            15,
            newLine.Quantity);

        Assert.Equal(
            200,
            newLine.UnitCost.Value);
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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrderId,
            DateTime.UtcNow,
            null,
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
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
    public async Task Handle_ShouldThrowDomainException_WhenPurchaseOrderIsPosted()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            Guid.NewGuid(),
            10,
            new Money(180));

        purchaseOrder.AddLine(line);
        purchaseOrder.Post();

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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            "Should Not Update",
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    line.ProductId,
                    20,
                    190)
            ]);

        await Assert.ThrowsAsync<PurchaseOrderDomainException>(
            () => handler.Handle(
                command,
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
    public async Task Handle_ShouldThrowDomainException_WhenPurchaseOrderIsCancelled()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            Guid.NewGuid(),
            10,
            new Money(180));

        purchaseOrder.AddLine(line);
        purchaseOrder.Cancel();

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
            new UpdatePurchaseOrderHandler(
                repository.Object,
                unitOfWork.Object);

        var command = new UpdatePurchaseOrderCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            "Should Not Update",
            null,
            [
                new UpdatePurchaseOrderLineRequest(
                    line.ProductId,
                    20,
                    190)
            ]);

        await Assert.ThrowsAsync<PurchaseOrderDomainException>(
            () => handler.Handle(
                command,
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

    private static PurchaseOrder CreatePurchaseOrder(
        DateTime? occurredAt = null)
    {
        return new PurchaseOrder(
            Guid.NewGuid(),
            occurredAt ?? DateTime.UtcNow,
            "PO-001",
            "Test");
    }
}