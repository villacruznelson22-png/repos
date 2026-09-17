using MediatR;
using WholesalePOS.Application.Customers.DTOs;

namespace WholesalePOS.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string? ContactNumber,
    string? Notes,
    CustomerAddressDto? Address
) : IRequest<Guid>;