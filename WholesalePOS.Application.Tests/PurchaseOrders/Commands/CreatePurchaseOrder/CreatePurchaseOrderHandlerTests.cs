using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreatePurchaseOrder()
    {
        var supplier = new Supplier(
            "Test Supplier");

        var repository = new Mock<ISupplierRepository>();
        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        PurchaseOrder? capturedPurchaseOrder = null;

        purchaseOrderRepository
            .Setup(x => x.AddAsync(
                It.IsAny<PurchaseOrder>(),
                It.IsAny<CancellationToken>()))
            .Callback<PurchaseOrder, CancellationToken>(
                (purchaseOrder, _) =>
                    capturedPurchaseOrder = purchaseOrder)
            .Returns(Task.CompletedTask);

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository.Object,
            repository.Object,
            unitOfWork.Object);

        var command = new CreatePurchaseOrderCommand(
            supplier.Id,
            DateTime.UtcNow,
            "PO-001",
            "Test notes",
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedPurchaseOrder);

        Assert.Equal(
            supplier.Id,
            capturedPurchaseOrder!.SupplierId);

        Assert.Single(
            capturedPurchaseOrder.Lines);

        Assert.Equal(
            10,
            capturedPurchaseOrder.Lines.Single().Quantity);

        Assert.Equal(
            180,
            capturedPurchaseOrder.Lines.Single().UnitCost.Value);

        purchaseOrderRepository.Verify(
            x => x.AddAsync(
                It.IsAny<PurchaseOrder>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenSupplierDoesNotExist()
    {
        var supplierId = Guid.NewGuid();

        var supplierRepository =
            new Mock<ISupplierRepository>();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        supplierRepository
            .Setup(x => x.GetByIdAsync(
                supplierId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository.Object,
            supplierRepository.Object,
            unitOfWork.Object);

        var command = new CreatePurchaseOrderCommand(
            supplierId,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        purchaseOrderRepository.Verify(
            x => x.AddAsync(
                It.IsAny<PurchaseOrder>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSupplierIsInactive()
    {
        var supplier = new Supplier(
            "Inactive Supplier");

        supplier.Deactivate();

        var supplierRepository =
            new Mock<ISupplierRepository>();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        supplierRepository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var handler = new CreatePurchaseOrderHandler(
            purchaseOrderRepository.Object,
            supplierRepository.Object,
            unitOfWork.Object);

        var command = new CreatePurchaseOrderCommand(
            supplier.Id,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreatePurchaseOrderLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        purchaseOrderRepository.Verify(
            x => x.AddAsync(
                It.IsAny<PurchaseOrder>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}