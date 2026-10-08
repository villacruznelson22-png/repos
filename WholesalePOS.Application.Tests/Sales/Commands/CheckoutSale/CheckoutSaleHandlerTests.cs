using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.CheckoutSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.CheckoutSale;

public class CheckoutSaleHandlerTests
{
    private readonly Mock<ISaleRepository> _saleRepositoryMock = new();
    private readonly Mock<IInventoryBalanceRepository> _inventoryBalanceRepositoryMock = new();
    private readonly Mock<IInventoryTransactionRepository> _inventoryTransactionRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldCompleteSaleAndConsumeInventory()
    {
        var product = CreateProduct();
        var sale = CreateConfirmedSale(product, quantity: 2, unitSellingPrice: 100);
        var balance = InventoryBalance.CreateOpeningBalance(
            product.Id,
            10,
            new InventoryCost(65));

        SetupSale(sale);
        _inventoryBalanceRepositoryMock
            .Setup(x => x.GetByProductIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var handler = CreateHandler();

        await handler.Handle(
            new CheckoutSaleCommand(
                sale.Id,
                "checkout-001",
                new[]
                {
                    new CheckoutPaymentRequest(
                        PaymentMethod.Cash,
                        200,
                        "payment-001")
                }),
            CancellationToken.None);

        Assert.Equal(SaleStatus.Completed, sale.Status);
        Assert.Equal(8, balance.QuantityOnHand);
        Assert.Equal(65, sale.Lines.Single().UnitCost!.Value);
        Assert.Single(sale.Payments);
        Assert.Equal("checkout-001", sale.CheckoutIdempotencyKey);

        _inventoryTransactionRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<WholesalePOS.Domain.Entities.InventoryTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectInsufficientInventory_WhenOverrideIsNotAllowed()
    {
        var product = CreateProduct();
        var sale = CreateConfirmedSale(product, quantity: 6, unitSellingPrice: 100);
        var balance = InventoryBalance.CreateOpeningBalance(
            product.Id,
            5,
            new InventoryCost(65));

        SetupSale(sale);
        _inventoryBalanceRepositoryMock
            .Setup(x => x.GetByProductIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                new CheckoutSaleCommand(
                    sale.Id,
                    "checkout-002",
                    new[]
                    {
                        new CheckoutPaymentRequest(
                            PaymentMethod.Cash,
                            600,
                            "payment-002")
                    }),
                CancellationToken.None));

        Assert.Equal(SaleStatus.Confirmed, sale.Status);
        Assert.Null(sale.Lines.Single().UnitCost);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithAuthorizedNegativeInventory_ShouldUseProductFallbackCost()
    {
        var product = CreateProduct(new InventoryCost(68));
        var sale = CreateConfirmedSale(product, quantity: 10, unitSellingPrice: 100);

        SetupSale(sale);
        _inventoryBalanceRepositoryMock
            .Setup(x => x.GetByProductIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryBalance?)null);

        var handler = CreateHandler();

        InventoryBalance? createdBalance = null;

        _inventoryBalanceRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<InventoryBalance>(),
                It.IsAny<CancellationToken>()))
            .Callback<InventoryBalance, CancellationToken>(
                (balance, _) => createdBalance = balance)
            .Returns(Task.CompletedTask);

        await handler.Handle(
            new CheckoutSaleCommand(
                sale.Id,
                "checkout-003",
                new[]
                {
                    new CheckoutPaymentRequest(
                        PaymentMethod.Cash,
                        1000,
                        "payment-003")
                },
                AllowNegativeInventory: true),
            CancellationToken.None);

        Assert.Equal(SaleStatus.Completed, sale.Status);
        Assert.NotNull(createdBalance);
        Assert.Equal(-10, createdBalance!.QuantityOnHand);
        Assert.Equal(-680, createdBalance.InventoryValue);
        Assert.Equal(68, sale.Lines.Single().UnitCost!.Value);
    }

    [Fact]
    public async Task Handle_ShouldRejectPaymentTotalMismatch()
    {
        var product = CreateProduct();
        var sale = CreateConfirmedSale(product, quantity: 2, unitSellingPrice: 100);

        SetupSale(sale);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                new CheckoutSaleCommand(
                    sale.Id,
                    "checkout-004",
                    new[]
                    {
                        new CheckoutPaymentRequest(
                            PaymentMethod.Cash,
                            150,
                            "payment-004")
                    }),
                CancellationToken.None));

        Assert.Equal(SaleStatus.Confirmed, sale.Status);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldTreatSameCompletedCheckoutKeyAsIdempotent()
    {
        var product = CreateProduct();
        var sale = CreateConfirmedSale(product, quantity: 1, unitSellingPrice: 100);
        sale.SetCheckoutIdempotencyKey("checkout-005");
        sale.Complete();

        SetupSale(sale);

        var handler = CreateHandler();

        await handler.Handle(
            new CheckoutSaleCommand(
                sale.Id,
                "checkout-005",
                Array.Empty<CheckoutPaymentRequest>()),
            CancellationToken.None);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _inventoryBalanceRepositoryMock.Verify(
            x => x.GetByProductIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupSale(Sale sale)
    {
        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
    }

    private CheckoutSaleHandler CreateHandler()
        => new(
            _saleRepositoryMock.Object,
            _inventoryBalanceRepositoryMock.Object,
            _inventoryTransactionRepositoryMock.Object,
            _unitOfWorkMock.Object);

    private static Product CreateProduct(
        InventoryCost? fallbackCost = null)
        => new(
            "Test Product",
            null,
            new Money(120),
            new Money(100),
            fallbackCost);

    private static Sale CreateConfirmedSale(
        Product product,
        decimal quantity,
        decimal unitSellingPrice)
    {
        var sale = new Sale(
            customerId: null,
            occurredAt: DateTime.UtcNow);

        var line = new SaleLine(
            sale.Id,
            product.Id,
            quantity,
            new Money(unitSellingPrice));

        typeof(SaleLine)
            .GetProperty(nameof(SaleLine.Product))!
            .SetValue(line, product);

        sale.AddLine(line);
        sale.Confirm();

        return sale;
    }
}
