using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Customers.Commands.UpdateCustomerAddress;
using WholesalePOS.Application.Customers.DTOs;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Customers.Commands.UpdateCustomerAddress;

public class UpdateCustomerAddressHandlerTests
{
    [Fact]
    public async Task ShouldUpdateExistingAddress()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var address = new CustomerAddress(
            customer.Id,
            "Home",
            "Old Street",
            Guid.NewGuid(),
            "1920",
            "Old Landmark",
            14.500000m,
            121.000000m,
            true);

        customer.AddAddress(address);

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var newBarangayId = Guid.NewGuid();

        var updatedAddress = new CustomerAddressDto(
            "Store",
            "New Street",
            newBarangayId,
            "1921",
            "New Landmark",
            14.600000m,
            121.100000m,
            true);

        var command = new UpdateCustomerAddressCommand(
            customer.Id,
            address.Id,
            updatedAddress);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal("Store", address.Label);
        Assert.Equal("New Street", address.Street);
        Assert.Equal(newBarangayId, address.BarangayId);
        Assert.Equal("1921", address.PostalCode);
        Assert.Equal("New Landmark", address.Landmark);
        Assert.Equal(14.600000m, address.Latitude);
        Assert.Equal(121.100000m, address.Longitude);
        Assert.True(address.IsDefault);

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

        var handler = new UpdateCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new UpdateCustomerAddressCommand(
            customerId,
            Guid.NewGuid(),
            new CustomerAddressDto(
                "Home",
                "Street",
                Guid.NewGuid(),
                null,
                null,
                null,
                null,
                false));

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

        var handler = new UpdateCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new UpdateCustomerAddressCommand(
            customer.Id,
            Guid.NewGuid(),
            new CustomerAddressDto(
                "Home",
                "Street",
                Guid.NewGuid(),
                null,
                null,
                null,
                null,
                false));

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
    public async Task ShouldRemoveDefault_WhenUpdatedAddressIsNoLongerDefault()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var address = new CustomerAddress(
            customer.Id,
            "Home",
            "Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            true);

        customer.AddAddress(address);

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var updatedAddress = new CustomerAddressDto(
            "Home",
            "Street",
            address.BarangayId,
            null,
            null,
            null,
            null,
            false);

        var command = new UpdateCustomerAddressCommand(
            customer.Id,
            address.Id,
            updatedAddress);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(address.IsDefault);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}