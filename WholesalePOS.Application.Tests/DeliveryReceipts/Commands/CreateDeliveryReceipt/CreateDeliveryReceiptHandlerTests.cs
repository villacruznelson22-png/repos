using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.DeliveryReceipts.Commands.CreateDeliveryReceipt;

public class CreateDeliveryReceiptHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateDeliveryReceipt_WhenPurchaseOrderIsPosted()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow,
            "PO-001",
            "Test purchase order");

        var product = new Product(
            "Coke Mismo",
            null,
            new Money(195),
            new Money(190));

        var purchaseOrderLine = new PurchaseOrderLine(
            purchaseOrder.Id,
            product.Id,
            20,
            new Money(180));

        purchaseOrder.AddLine(purchaseOrderLine);
        purchaseOrder.Post();

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            "DR-001",
            "Actual delivery",
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    product.Id,
                    18,
                    182,
                    new DateTime(2027, 1, 31))
            });

        DeliveryReceipt? capturedDeliveryReceipt = null;

        deliveryReceiptRepository
            .Setup(x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()))
            .Callback<DeliveryReceipt, CancellationToken>(
                (deliveryReceipt, _) =>
                {
                    capturedDeliveryReceipt = deliveryReceipt;
                })
            .Returns(Task.CompletedTask);

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedDeliveryReceipt);

        Assert.Equal(result, capturedDeliveryReceipt!.Id);
        Assert.Equal(
            purchaseOrder.Id,
            capturedDeliveryReceipt.PurchaseOrderId);

        Assert.Equal(
            command.OccurredAt,
            capturedDeliveryReceipt.OccurredAt);

        Assert.Equal(
            "DR-001",
            capturedDeliveryReceipt.ReferenceNumber);

        Assert.Equal(
            "Actual delivery",
            capturedDeliveryReceipt.Notes);

        Assert.Equal(
            DeliveryReceiptStatus.Draft,
            capturedDeliveryReceipt.Status);

        var line = Assert.Single(capturedDeliveryReceipt.Lines);

        Assert.Equal(product.Id, line.ProductId);
        Assert.Equal(18, line.Quantity);
        Assert.Equal(182, line.UnitCost.Value);
        Assert.Equal(
            new DateTime(2027, 1, 31),
            line.ExpirationDate);

        deliveryReceiptRepository.Verify(
            x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()),
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

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        var productRepository =
            new Mock<IProductRepository>();

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrderId,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    Guid.NewGuid(),
                    10,
                    180,
                    null)
            });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        deliveryReceiptRepository.Verify(
            x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPurchaseOrderIsDraft()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        var purchaseOrderProduct = new Product(
            "Sprite",
            null,
            new Money(195),
            new Money(190));

        purchaseOrder.AddLine(
            new PurchaseOrderLine(
                purchaseOrder.Id,
                purchaseOrderProduct.Id,
                100,
                new Money(180)));

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    Guid.NewGuid(),
                    10,
                    180,
                    null)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        productRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPurchaseOrderIsCancelled()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        purchaseOrder.Cancel();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    Guid.NewGuid(),
                    10,
                    180,
                    null)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        productRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        var purchaseOrderProduct = new Product(
            "Sprite",
            null,
            new Money(195),
            new Money(190));

        purchaseOrder.AddLine(
            new PurchaseOrderLine(
                purchaseOrder.Id,
                purchaseOrderProduct.Id,
                100,
                new Money(180)));

        purchaseOrder.Post();

        var productId = Guid.NewGuid();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    productId,
                    10,
                    180,
                    null)
            });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        deliveryReceiptRepository.Verify(
            x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductIsInactive()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        var purchaseOrderProduct = new Product(
            "Sprite",
            null,
            new Money(195),
            new Money(190));

        purchaseOrder.AddLine(
            new PurchaseOrderLine(
                purchaseOrder.Id,
                purchaseOrderProduct.Id,
                100,
                new Money(180)));

        purchaseOrder.Post();

        var product = new Product(
            "Coke Mismo",
            null,
            new Money(195),
            new Money(190));

        product.Deactivate();

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    product.Id,
                    10,
                    180,
                    null)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        deliveryReceiptRepository.Verify(
            x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAllowProductNotPresentOnPurchaseOrder()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        var purchaseOrderProduct = new Product(
            "Sprite",
            null,
            new Money(195),
            new Money(190));

        purchaseOrder.AddLine(
            new PurchaseOrderLine(
                purchaseOrder.Id,
                purchaseOrderProduct.Id,
                100,
                new Money(180)));

        purchaseOrder.Post();

        var product = new Product(
            "Royal",
            null,
            new Money(195),
            new Money(190));

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        DeliveryReceipt? capturedDeliveryReceipt = null;

        deliveryReceiptRepository
            .Setup(x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()))
            .Callback<DeliveryReceipt, CancellationToken>(
                (deliveryReceipt, _) =>
                {
                    capturedDeliveryReceipt = deliveryReceipt;
                })
            .Returns(Task.CompletedTask);

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    product.Id,
                    10,
                    180,
                    null)
            });

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(capturedDeliveryReceipt);

        var line = Assert.Single(capturedDeliveryReceipt!.Lines);

        Assert.Equal(product.Id, line.ProductId);
        Assert.Equal(10, line.Quantity);
        Assert.Equal(180, line.UnitCost.Value);
    }

    [Fact]
    public async Task Handle_ShouldAllowMultipleLinesForSameProduct()
    {
        var supplier = new Supplier("Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow);

        var purchaseOrderProduct = new Product(
            "Sprite",
            null,
            new Money(195),
            new Money(190));

        purchaseOrder.AddLine(
            new PurchaseOrderLine(
                purchaseOrder.Id,
                purchaseOrderProduct.Id,
                100,
                new Money(180)));

        purchaseOrder.Post();

        var product = new Product(
            "Coke Mismo",
            null,
            new Money(195),
            new Money(190));

        var purchaseOrderRepository =
            new Mock<IPurchaseOrderRepository>();

        purchaseOrderRepository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var productRepository =
            new Mock<IProductRepository>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var deliveryReceiptRepository =
            new Mock<IDeliveryReceiptRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        DeliveryReceipt? capturedDeliveryReceipt = null;

        deliveryReceiptRepository
            .Setup(x => x.AddAsync(
                It.IsAny<DeliveryReceipt>(),
                It.IsAny<CancellationToken>()))
            .Callback<DeliveryReceipt, CancellationToken>(
                (deliveryReceipt, _) =>
                {
                    capturedDeliveryReceipt = deliveryReceipt;
                })
            .Returns(Task.CompletedTask);

        var handler = new CreateDeliveryReceiptHandler(
            purchaseOrderRepository.Object,
            productRepository.Object,
            deliveryReceiptRepository.Object,
            unitOfWork.Object);

        var command = new CreateDeliveryReceiptCommand(
            purchaseOrder.Id,
            DateTime.UtcNow,
            null,
            null,
            new[]
            {
                new CreateDeliveryReceiptLineRequest(
                    product.Id,
                    10,
                    180,
                    new DateTime(2027, 1, 31)),

                new CreateDeliveryReceiptLineRequest(
                    product.Id,
                    15,
                    182,
                    new DateTime(2027, 3, 31))
            });

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(capturedDeliveryReceipt);
        Assert.Equal(2, capturedDeliveryReceipt!.Lines.Count);

        Assert.Equal(
            product.Id,
            capturedDeliveryReceipt.Lines.ElementAt(0).ProductId);

        Assert.Equal(
            product.Id,
            capturedDeliveryReceipt.Lines.ElementAt(1).ProductId);

        productRepository.Verify(
            x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}