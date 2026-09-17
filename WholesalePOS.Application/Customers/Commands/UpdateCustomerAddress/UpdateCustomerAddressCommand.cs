using MediatR;
using WholesalePOS.Application.Customers.DTOs;

namespace WholesalePOS.Application.Customers.Commands.UpdateCustomerAddress;

public record UpdateCustomerAddressCommand(
    Guid CustomerId,
    Guid AddressId,
    CustomerAddressDto Address
) : IRequest;