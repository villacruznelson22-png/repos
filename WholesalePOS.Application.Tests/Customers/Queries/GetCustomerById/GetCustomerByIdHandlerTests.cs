using Moq;
using WholesalePOS.Application.Customers.Queries.GetCustomerById;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Tests.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCustomer()
    {
        // Arrange
        var customerRepository =
            new Mock<ICustomerRepository>();

        var customer =
            new Customer(
                "Juan Dela Cruz",
                "09171234567",
                "Wholesale customer");

        customerRepository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var handler =
            new GetCustomerByIdHandler(
                customerRepository.Object);

        var query =
            new GetCustomerByIdQuery
            {
                Id = customer.Id
            };

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.Equal(
            customer.Id,
            result.Id);

        Assert.Equal(
            "Juan Dela Cruz",
            result.Name);

        Assert.Equal(
            "09171234567",
            result.ContactNumber);

        Assert.Equal(
            "Wholesale customer",
            result.Notes);

        Assert.True(result.IsActive);

        Assert.Empty(result.Addresses);

        customerRepository.Verify(
            x => x.GetByIdWithAddressesAsync(
                customer.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerRepository =
            new Mock<ICustomerRepository>();

        var customerId =
            Guid.NewGuid();

        customerRepository
            .Setup(x => x.GetByIdWithAddressesAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler =
            new GetCustomerByIdHandler(
                customerRepository.Object);

        var query =
            new GetCustomerByIdQuery
            {
                Id = customerId
            };

        // Act
        var action = () =>
            handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            WholesalePOS.Application.Common.Exceptions.NotFoundException>(
            action);

        customerRepository.Verify(
            x => x.GetByIdWithAddressesAsync(
                customerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}