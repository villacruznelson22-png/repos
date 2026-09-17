using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Customers.Commands.RemoveCustomerAddress;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Customers.Commands.RemoveCustomerAddress;

public class RemoveCustomerAddressHandlerTests
{
    [Fact]
    public async Task ShouldRemoveExistingAddress()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var address = new CustomerAddress(
            customer.Id,
            "Home",
            "Home Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            false);

        customer.AddAddress(address);

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new RemoveCustomerAddressCommand(
            customer.Id,
            address.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Empty(customer.Addresses);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldRemoveDefaultAddress_WithoutMakingAnotherAddressDefault()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var defaultAddress = new CustomerAddress(
            customer.Id,
            "Home",
            "Home Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            true);

        var otherAddress = new CustomerAddress(
            customer.Id,
            "Store",
            "Store Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            false);

        customer.AddAddress(defaultAddress);
        customer.AddAddress(otherAddress);

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new RemoveCustomerAddressCommand(
            customer.Id,
            defaultAddress.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Single(customer.Addresses);
        Assert.Equal(otherAddress.Id, customer.Addresses.Single().Id);
        Assert.False(customer.Addresses.Single().IsDefault);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new RemoveCustomerAddressCommand(
            customerId,
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ShouldThrowNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RemoveCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new RemoveCustomerAddressCommand(
            customer.Id,
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}