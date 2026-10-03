using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.CreateSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.CreateSale;

public class CreateSaleHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateWalkInSale()
    {
        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(80),
            new Money(75));

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Sale? capturedSale = null;

        saleRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>(
                (sale, _) =>
                    capturedSale = sale)
            .Returns(Task.CompletedTask);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            null,
            DateTime.UtcNow,
            "SALE-001",
            "Walk-in sale",
            [
                new CreateSaleLineRequest(
                    product.Id,
                    10,
                    75)
            ]);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedSale);

        Assert.Equal(
            result,
            capturedSale!.Id);

        Assert.Null(
            capturedSale.CustomerId);

        Assert.Equal(
            "SALE-001",
            capturedSale.ReferenceNumber);

        Assert.Single(
            capturedSale.Lines);

        var line = capturedSale.Lines.Single();

        Assert.Equal(
            product.Id,
            line.ProductId);

        Assert.Equal(
            10,
            line.Quantity);

        Assert.Equal(
            75,
            line.UnitSellingPrice.Value);

        saleRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomerSale()
    {
        var customer = new Customer(
            "Juan Dela Cruz",
            "09171234567",
            null);

        var product = new Product(
            "Sprite 1.5L",
            null,
            new Money(80),
            new Money(75));

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        customerRepository
            .Setup(x => x.GetByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Sale? capturedSale = null;

        saleRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>(
                (sale, _) =>
                    capturedSale = sale)
            .Returns(Task.CompletedTask);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            customer.Id,
            DateTime.UtcNow,
            "SALE-002",
            "Customer sale",
            [
                new CreateSaleLineRequest(
                    product.Id,
                    20,
                    74)
            ]);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedSale);

        Assert.Equal(
            customer.Id,
            capturedSale!.CustomerId);

        Assert.Single(
            capturedSale.Lines);

        Assert.Equal(
            20,
            capturedSale.Lines.Single().Quantity);

        Assert.Equal(
            74,
            capturedSale.Lines.Single().UnitSellingPrice.Value);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        customerRepository
            .Setup(x => x.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            customerId,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateSaleLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        saleRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCustomerIsInactive()
    {
        var customer = new Customer(
            "Inactive Customer");

        customer.Deactivate();

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        customerRepository
            .Setup(x => x.GetByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            customer.Id,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateSaleLineRequest(
                    Guid.NewGuid(),
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        saleRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            null,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateSaleLineRequest(
                    productId,
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        saleRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Sale>(),
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
        var product = new Product(
            "Inactive Product",
            null,
            new Money(195),
            new Money(190));

        product.Deactivate();

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            null,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateSaleLineRequest(
                    product.Id,
                    10,
                    180)
            ]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        saleRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAllowMultipleLinesForSameProduct()
    {
        var product = new Product(
            "Coca-Cola 1.5L",
            null,
            new Money(80),
            new Money(75));

        var customerRepository =
            new Mock<ICustomerRepository>();

        var productRepository =
            new Mock<IProductRepository>();

        var saleRepository =
            new Mock<ISaleRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Sale? capturedSale = null;

        saleRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Sale>(),
                It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>(
                (sale, _) =>
                    capturedSale = sale)
            .Returns(Task.CompletedTask);

        var handler = new CreateSaleHandler(
            customerRepository.Object,
            productRepository.Object,
            saleRepository.Object,
            unitOfWork.Object);

        var command = new CreateSaleCommand(
            null,
            DateTime.UtcNow,
            null,
            null,
            [
                new CreateSaleLineRequest(
                    product.Id,
                    10,
                    75),

                new CreateSaleLineRequest(
                    product.Id,
                    5,
                    70)
            ]);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(capturedSale);

        Assert.Equal(
            2,
            capturedSale!.Lines.Count);

        Assert.Equal(
            10,
            capturedSale.Lines.ElementAt(0).Quantity);

        Assert.Equal(
            75,
            capturedSale.Lines.ElementAt(0).UnitSellingPrice.Value);

        Assert.Equal(
            5,
            capturedSale.Lines.ElementAt(1).Quantity);

        Assert.Equal(
            70,
            capturedSale.Lines.ElementAt(1).UnitSellingPrice.Value);

        productRepository.Verify(
            x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}