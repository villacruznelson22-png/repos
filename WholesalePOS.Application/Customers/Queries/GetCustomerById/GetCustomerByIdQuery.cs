using MediatR;

namespace WholesalePOS.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQuery : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
}