using Moq;
using WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Services;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.DeliveryReceipts.Commands.PostDeliveryReceipt;

public class PostDeliveryReceiptHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPostReceiptAndCreateInventoryForNewProduct()
    {
        var productId = Guid.NewGuid();
        var deliveryReceipt = CreateDeliveryReceipt(
            Guid.NewGuid(),
            (productId, 10, 65));

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var inventoryBalanceRepository =
            new Mock<IInventoryBalanceRepository>();

        var inventoryTransactionRepository =
            new Mock<IInventoryTransactionRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        InventoryBalance? capturedBalance = null;
        InventoryTransaction? capturedTransaction = null;

        deliveryReceiptRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                deliveryReceipt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryReceipt);

        inventoryBalanceRepository
            .Setup(x => x.GetByProductIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryBalance?)null);

        inventoryBalanceRepository
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()))
            .Callback<InventoryBalance, CancellationToken>(
                (balance, _) => capturedBalance = balance)
            .Returns(Task.CompletedTask);

        inventoryTransactionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()))
            .Callback<InventoryTransaction, CancellationToken>(
                (transaction, _) => capturedTransaction = transaction)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(
            deliveryReceiptRepository,
            inventoryBalanceRepository,
            inventoryTransactionRepository,
            unitOfWork);

        await handler.Handle(
            new PostDeliveryReceiptCommand(deliveryReceipt.Id),
            CancellationToken.None);

        Assert.Equal(
            DeliveryReceiptStatus.Posted,
            deliveryReceipt.Status);

        Assert.NotNull(capturedBalance);
        Assert.Equal(10, capturedBalance!.QuantityOnHand);
        Assert.Equal(650, capturedBalance.InventoryValue);
        Assert.Equal(65, capturedBalance.AverageUnitCost.Value);

        Assert.NotNull(capturedTransaction);
        Assert.Equal(
            InventoryTransactionType.Purchase,
            capturedTransaction!.Type);
        Assert.Equal(
            InventoryTransactionDirection.Increase,
            capturedTransaction.Direction);
        Assert.Equal(10, capturedTransaction.Quantity!.Value);
        Assert.Equal(65, capturedTransaction.UnitCost!.Value);
        Assert.Equal(650, capturedTransaction.TotalCost);
        Assert.Equal(
            "DeliveryReceipt",
            capturedTransaction.ReferenceType);
        Assert.Equal(
            deliveryReceipt.Id,
            capturedTransaction.ReferenceId);

        inventoryBalanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        inventoryTransactionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingInventoryBalance()
    {
        var productId = Guid.NewGuid();

        var deliveryReceipt = CreateDeliveryReceipt(
            Guid.NewGuid(),
            (productId, 10, 70));

        var existingBalance =
            InventoryBalance.CreateOpeningBalance(
                productId,
                100,
                new InventoryCost(65));

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var inventoryBalanceRepository =
            new Mock<IInventoryBalanceRepository>();

        var inventoryTransactionRepository =
            new Mock<IInventoryTransactionRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        deliveryReceiptRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                deliveryReceipt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryReceipt);

        inventoryBalanceRepository
            .Setup(x => x.GetByProductIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);

        inventoryTransactionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(
            deliveryReceiptRepository,
            inventoryBalanceRepository,
            inventoryTransactionRepository,
            unitOfWork);

        await handler.Handle(
            new PostDeliveryReceiptCommand(deliveryReceipt.Id),
            CancellationToken.None);

        Assert.Equal(110, existingBalance.QuantityOnHand);
        Assert.Equal(1350 + 5850, existingBalance.InventoryValue);
        Assert.Equal(
            65.454545m,
            existingBalance.AverageUnitCost.Value);

        inventoryBalanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldApplyMultipleLinesForSameProductSequentially()
    {
        var productId = Guid.NewGuid();

        var deliveryReceipt = CreateDeliveryReceipt(
            Guid.NewGuid(),
            (productId, 10, 65),
            (productId, 5, 70));

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var inventoryBalanceRepository =
            new Mock<IInventoryBalanceRepository>();

        var inventoryTransactionRepository =
            new Mock<IInventoryTransactionRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        InventoryBalance? capturedBalance = null;

        deliveryReceiptRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                deliveryReceipt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryReceipt);

        inventoryBalanceRepository
            .Setup(x => x.GetByProductIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryBalance?)null);

        inventoryBalanceRepository
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()))
            .Callback<InventoryBalance, CancellationToken>(
                (balance, _) => capturedBalance = balance)
            .Returns(Task.CompletedTask);

        inventoryTransactionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(
            deliveryReceiptRepository,
            inventoryBalanceRepository,
            inventoryTransactionRepository,
            unitOfWork);

        await handler.Handle(
            new PostDeliveryReceiptCommand(deliveryReceipt.Id),
            CancellationToken.None);

        Assert.NotNull(capturedBalance);
        Assert.Equal(15, capturedBalance!.QuantityOnHand);
        Assert.Equal(1000, capturedBalance.InventoryValue);
        Assert.Equal(
            66.666667m,
            capturedBalance.AverageUnitCost.Value);

        inventoryBalanceRepository.Verify(
            x => x.GetByProductIdAsync(
                productId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        inventoryBalanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        inventoryTransactionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldNotPostAgain_WhenReceiptIsAlreadyPosted()
    {
        var deliveryReceipt = CreateDeliveryReceipt(
            Guid.NewGuid(),
            (Guid.NewGuid(), 10, 65));

        deliveryReceipt.Post();

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var inventoryBalanceRepository =
            new Mock<IInventoryBalanceRepository>();

        var inventoryTransactionRepository =
            new Mock<IInventoryTransactionRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        deliveryReceiptRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                deliveryReceipt.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryReceipt);

        var handler = CreateHandler(
            deliveryReceiptRepository,
            inventoryBalanceRepository,
            inventoryTransactionRepository,
            unitOfWork);

        await Assert.ThrowsAsync<
            WholesalePOS.Domain.Exceptions.DeliveryReceiptDomainException>(
            () => handler.Handle(
                new PostDeliveryReceiptCommand(deliveryReceipt.Id),
                CancellationToken.None));

        inventoryBalanceRepository.Verify(
            x => x.GetByProductIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        inventoryTransactionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<InventoryTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static PostDeliveryReceiptHandler CreateHandler(
        Mock<IDeliveryReceiptRepository> deliveryReceiptRepository,
        Mock<IInventoryBalanceRepository> inventoryBalanceRepository,
        Mock<IInventoryTransactionRepository> inventoryTransactionRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new PostDeliveryReceiptHandler(
            deliveryReceiptRepository.Object,
            inventoryBalanceRepository.Object,
            inventoryTransactionRepository.Object,
            new InventoryService(),
            unitOfWork.Object);
    }

    private static DeliveryReceipt CreateDeliveryReceipt(
        Guid purchaseOrderId,
        params (Guid ProductId, decimal Quantity, decimal UnitCost)[] lines)
    {
        var deliveryReceipt = new DeliveryReceipt(
            purchaseOrderId,
            DateTime.UtcNow);

        foreach (var line in lines)
        {
            deliveryReceipt.AddLine(
                new DeliveryReceiptLine(
                    deliveryReceipt.Id,
                    line.ProductId,
                    line.Quantity,
                    new Money(line.UnitCost)));
        }

        return deliveryReceipt;
    }
}
