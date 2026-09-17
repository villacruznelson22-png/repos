using Moq;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.ActivateSupplier;

public class ActivateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_ShouldActivateSupplier()
    {
        var supplier = new Supplier("Supplier");
        supplier.Deactivate();

        var repository = new Mock<ISupplierRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var handler = new ActivateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new ActivateSupplierCommand(
            supplier.Id);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(supplier.IsActive);

        repository.Verify(
            x => x.Update(supplier),
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

        var repository = new Mock<ISupplierRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplierId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new ActivateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new ActivateSupplierCommand(
            supplierId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        repository.Verify(
            x => x.Update(It.IsAny<Supplier>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}