using Castle.Core.Resource;
using Moq;
using WholesalePOS.Application.Customers.Commands.CreateCustomer;
using WholesalePOS.Application.Customers.DTOs;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Customers.Commands.CreateCustomer;

public class CreateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCustomerWithoutAddress()
    {
        // Arrange
        var customerRepository =
            new Mock<ICustomerRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateCustomerHandler(
            customerRepository.Object,
            unitOfWork.Object);

        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer",
            null);

        Customer? savedCustomer = null;

        customerRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>(
                (customer, _) => savedCustomer = customer)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var customerId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, customerId);
        Assert.NotNull(savedCustomer);

        Assert.Equal(
            customerId,
            savedCustomer!.Id);

        Assert.Equal(
            "Juan Dela Cruz",
            savedCustomer.Name);

        Assert.Equal(
            "09171234567",
            savedCustomer.ContactNumber);

        Assert.Equal(
            "Wholesale customer",
            savedCustomer.Notes);

        Assert.Empty(savedCustomer.Addresses);

        customerRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomerWithAddress()
    {
        // Arrange
        var customerRepository =
            new Mock<ICustomerRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateCustomerHandler(
            customerRepository.Object,
            unitOfWork.Object);

        var barangayId = Guid.NewGuid();

        var address = new CustomerAddressDto(
            "Home",
            "123 Mabini Street",
            barangayId,
            "1920",
            "Near the church",
            14.5631m,
            121.1324m,
            true);

        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            "09171234567",
            "Wholesale customer",
            address);

        Customer? savedCustomer = null;

        customerRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>(
                (customer, _) => savedCustomer = customer)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var customerId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotNull(savedCustomer);

        Assert.Equal(
            customerId,
            savedCustomer!.Id);

        var savedAddress =
            Assert.Single(savedCustomer.Addresses);

        Assert.Equal(
            "Home",
            savedAddress.Label);

        Assert.Equal(
            "123 Mabini Street",
            savedAddress.Street);

        Assert.Equal(
            barangayId,
            savedAddress.BarangayId);

        Assert.Equal(
            "1920",
            savedAddress.PostalCode);

        Assert.Equal(
            "Near the church",
            savedAddress.Landmark);

        Assert.Equal(
            14.5631m,
            savedAddress.Latitude);

        Assert.Equal(
            121.1324m,
            savedAddress.Longitude);

        Assert.True(savedAddress.IsDefault);

        customerRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToDependencies()
    {
        // Arrange
        var customerRepository =
            new Mock<ICustomerRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        var handler = new CreateCustomerHandler(
            customerRepository.Object,
            unitOfWork.Object);

        var cancellationToken =
            new CancellationTokenSource().Token;

        var command = new CreateCustomerCommand(
            "Juan Dela Cruz",
            null,
            null,
            null);

        customerRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(
            command,
            cancellationToken);

        // Assert
        customerRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                cancellationToken),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }
}