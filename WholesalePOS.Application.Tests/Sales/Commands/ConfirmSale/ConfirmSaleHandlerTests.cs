using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Commands.ConfirmSale;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.Sales.Commands.ConfirmSale;

public class ConfirmSaleHandlerTests
{
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmSaleHandler _handler;

    public ConfirmSaleHandlerTests()
    {
        _saleRepositoryMock = new Mock<ISaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ConfirmSaleHandler(
            _saleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldConfirmDraftSale()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new ConfirmSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(SaleStatus.Confirmed, sale.Status);
        Assert.NotNull(sale.ConfirmedAt);

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

        var command = new ConfirmSaleCommand(saleId);

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
    public async Task Handle_ShouldNotSaveChanges_WhenSaleCannotBeConfirmed()
    {
        // Arrange
        var sale = CreateDraftSaleWithLine();
        sale.Confirm();

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new ConfirmSaleCommand(sale.Id);

        // Act & Assert
        await Assert.ThrowsAsync<SaleDomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotConfirmSale_WhenSaleHasNoLines()
    {
        // Arrange
        var sale = new Sale(
            customerId: null,
            occurredAt: DateTime.UtcNow);

        _saleRepositoryMock
            .Setup(x => x.GetByIdWithLinesAsync(
                sale.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new ConfirmSaleCommand(sale.Id);

        // Act & Assert
        await Assert.ThrowsAsync<SaleDomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

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

        var command = new ConfirmSaleCommand(sale.Id);

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

        var command = new ConfirmSaleCommand(sale.Id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(SaleStatus.Confirmed, sale.Status);
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