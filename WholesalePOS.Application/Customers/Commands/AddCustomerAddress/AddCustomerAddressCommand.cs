using MediatR;
using WholesalePOS.Application.Customers.DTOs;

namespace WholesalePOS.Application.Customers.Commands.AddCustomerAddress;

public record AddCustomerAddressCommand(
    Guid CustomerId,
    CustomerAddressDto Address
) : IRequest<Guid>;