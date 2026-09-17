using Moq;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.DeactivateSupplier;

public class DeactivateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeactivateSupplier()
    {
        var supplier = new Supplier("Supplier");

        var repository = new Mock<ISupplierRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var handler = new DeactivateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new DeactivateSupplierCommand(
            supplier.Id);

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(supplier.IsActive);

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

        var handler = new DeactivateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new DeactivateSupplierCommand(
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