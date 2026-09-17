using Moq;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Suppliers.Commands.UpdateSupplier;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSupplier()
    {
        var supplier = new Supplier(
            "Old Supplier",
            "09111111111",
            "Old Address",
            "Old Notes");

        var repository = new Mock<ISupplierRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var handler = new UpdateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new UpdateSupplierCommand(
            supplier.Id,
            "Updated Supplier",
            "09222222222",
            "Updated Address",
            "Updated Notes");

        await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal("Updated Supplier", supplier.Name);
        Assert.Equal("09222222222", supplier.ContactNumber);
        Assert.Equal("Updated Address", supplier.Address);
        Assert.Equal("Updated Notes", supplier.Notes);

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

        var handler = new UpdateSupplierHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new UpdateSupplierCommand(
            supplierId,
            "Supplier",
            null,
            null,
            null);

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