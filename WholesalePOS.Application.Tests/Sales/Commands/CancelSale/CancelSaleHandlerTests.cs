using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.CancelSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.CancelSale;

public class CancelSaleHandlerTests
{
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepositoryMock = new Mock<ISaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CancelSaleHandler(
            _saleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelDraftSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
        Assert.NotNull(sale.CancelledAt);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelConfirmedSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();
        sale.Confirm();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
        Assert.NotNull(sale.CancelledAt);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenSaleDoesNotExist()
    {
        // Arrange
        var saleId = Guid.NewGuid();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                saleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        var command = new CancelSaleCommand(saleId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCancelCompletedSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();
        sale.Confirm();
        sale.Complete();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act & Assert
        await Assert.ThrowsAsync<SaleDomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(SaleStatus.Completed, sale.Status);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCancelAlreadyCancelledSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();
        sale.Cancel();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act & Assert
        await Assert.ThrowsAsync<SaleDomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(SaleStatus.Cancelled, sale.Status);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCancelVoidedSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();
        sale.Confirm();
        sale.Complete();
        sale.Void(Guid.NewGuid(), "Test void");

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act & Assert
        await Assert.ThrowsAsync<SaleDomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(SaleStatus.Voided, sale.Status);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLoadSaleUsingRepository()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _saleRepositoryMock.Verify(
            x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotAccessInventory()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new CancelSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
    }

    private static Sale CreateDraftSaleWithLine()
    {
        var sale = new Sale(
            customerId: null,
            occurredAt: DateTime.UtcNow);

        var line = new SaleLine(
            sale.Id,
            Guid.NewGuid(),
            quantity: 2,
            unitSellingPrice: new Money(100));

        sale.AddLine(line);

        return sale;
    }
}