using MediatR;

namespace WholesalePOS.Application.Customers.Commands.RemoveCustomerAddress;

public record RemoveCustomerAddressCommand(
    Guid CustomerId,
    Guid AddressId
) : IRequest;