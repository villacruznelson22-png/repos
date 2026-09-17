using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Customers.Commands.AddCustomerAddress;
using WholesalePOS.Application.Customers.DTOs;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Customers.Commands.AddCustomerAddress;

public class AddCustomerAddressHandlerTests
{
    [Fact]
    public async Task ShouldAddAddressToExistingCustomer()
    {
        // Arrange
        var customer = new Customer(
            "Test Customer",
            "09171234567",
            "Test notes");

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new AddCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var address = new CustomerAddressDto(
            "Home",
            "123 Test Street",
            Guid.NewGuid(),
            "1920",
            "Near the store",
            14.5764m,
            121.1325m,
            true);

        var command = new AddCustomerAddressCommand(
            customer.Id,
            address);

        // Act
        var addressId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, addressId);
        Assert.Single(customer.Addresses);

        var addedAddress = customer.Addresses.Single();

        Assert.Equal(addressId, addedAddress.Id);
        Assert.Equal("Home", addedAddress.Label);
        Assert.Equal("123 Test Street", addedAddress.Street);
        Assert.Equal(address.BarangayId, addedAddress.BarangayId);
        Assert.Equal("1920", addedAddress.PostalCode);
        Assert.Equal("Near the store", addedAddress.Landmark);
        Assert.Equal(14.5764m, addedAddress.Latitude);
        Assert.Equal(121.1325m, addedAddress.Longitude);
        Assert.True(addedAddress.IsDefault);

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

        var handler = new AddCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var address = new CustomerAddressDto(
            "Home",
            "123 Test Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            true);

        var command = new AddCustomerAddressCommand(
            customerId,
            address);

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
    public async Task ShouldPassCancellationTokenToRepository()
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

        var handler = new AddCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var cancellationToken = new CancellationToken();

        var address = new CustomerAddressDto(
            "Home",
            "123 Test Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            false);

        var command = new AddCustomerAddressCommand(
            customer.Id,
            address);

        // Act
        await handler.Handle(
            command,
            cancellationToken);

        // Assert
        repository.Verify(
            x => x.GetByIdWithAddressesAsync(
                customer.Id,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ShouldMakeNewAddressDefault_WhenExistingDefaultAddressExists()
    {
        // Arrange
        var customer = new Customer("Test Customer");

        var existingAddress = new CustomerAddress(
            customer.Id,
            "Old Home",
            "Old Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            true);

        customer.AddAddress(existingAddress);

        var repository = new Mock<ICustomerRepository>();

        repository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new AddCustomerAddressHandler(
            repository.Object,
            unitOfWork.Object);

        var newAddress = new CustomerAddressDto(
            "New Home",
            "New Street",
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            true);

        var command = new AddCustomerAddressCommand(
            customer.Id,
            newAddress);

        // Act
        var newAddressId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, customer.Addresses.Count);

        var oldAddress = customer.Addresses
            .Single(x => x.Id == existingAddress.Id);

        var addedAddress = customer.Addresses
            .Single(x => x.Id == newAddressId);

        Assert.False(oldAddress.IsDefault);
        Assert.True(addedAddress.IsDefault);

        Assert.Single(
            customer.Addresses.Where(x => x.IsDefault));
    }
}