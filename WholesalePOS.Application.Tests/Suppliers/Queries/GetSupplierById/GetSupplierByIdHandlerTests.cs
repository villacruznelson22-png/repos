using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Suppliers.Queries.GetSupplierById;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSupplierDto_WhenSupplierExists()
    {
        var supplier = new Supplier(
            "JTI Philippines",
            "09170000000",
            "Manila",
            "Cigarette supplier");

        var repository =
            new Mock<ISupplierRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                CancellationToken.None))
            .ReturnsAsync(supplier);

        var handler =
            new GetSupplierByIdHandler(
                repository.Object);

        var result = await handler.Handle(
            new GetSupplierByIdQuery(supplier.Id),
            CancellationToken.None);

        Assert.Equal(supplier.Id, result.Id);
        Assert.Equal("JTI Philippines", result.Name);
        Assert.Equal("09170000000", result.ContactNumber);
        Assert.Equal("Manila", result.Address);
        Assert.Equal("Cigarette supplier", result.Notes);
        Assert.True(result.IsActive);
        Assert.Equal(supplier.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenSupplierDoesNotExist()
    {
        var supplierId = Guid.NewGuid();

        var repository =
            new Mock<ISupplierRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                supplierId,
                CancellationToken.None))
            .ReturnsAsync((Supplier?)null);

        var handler =
            new GetSupplierByIdHandler(
                repository.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new GetSupplierByIdQuery(supplierId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        var supplier = new Supplier(
            "JTI Philippines");

        var repository =
            new Mock<ISupplierRepository>();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        repository
            .Setup(x => x.GetByIdAsync(
                supplier.Id,
                cancellationToken))
            .ReturnsAsync(supplier);

        var handler =
            new GetSupplierByIdHandler(
                repository.Object);

        await handler.Handle(
            new GetSupplierByIdQuery(supplier.Id),
            cancellationToken);

        repository.Verify(
            x => x.GetByIdAsync(
                supplier.Id,
                cancellationToken),
            Times.Once);
    }
}