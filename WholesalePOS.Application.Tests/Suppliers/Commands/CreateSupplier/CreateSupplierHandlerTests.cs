using Moq;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Suppliers.Commands.CreateSupplier;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSupplierAndSaveChanges()
    {
        var supplierRepository =
            new Mock<ISupplierRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler =
            new CreateSupplierHandler(
                supplierRepository.Object,
                unitOfWork.Object);

        var command = new CreateSupplierCommand(
            "JTI Philippines",
            "09170000000",
            "Manila",
            "Cigarette supplier");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);

        supplierRepository.Verify(
            x => x.AddAsync(
                It.Is<Supplier>(supplier =>
                    supplier.Id == result &&
                    supplier.Name == "JTI Philippines" &&
                    supplier.ContactNumber == "09170000000" &&
                    supplier.Address == "Manila" &&
                    supplier.Notes == "Cigarette supplier" &&
                    supplier.IsActive),
                CancellationToken.None),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepositoryAndUnitOfWork()
    {
        var supplierRepository =
            new Mock<ISupplierRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler =
            new CreateSupplierHandler(
                supplierRepository.Object,
                unitOfWork.Object);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var command = new CreateSupplierCommand(
            "JTI Philippines",
            null,
            null,
            null);

        await handler.Handle(
            command,
            cancellationToken);

        supplierRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Supplier>(),
                cancellationToken),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }
}