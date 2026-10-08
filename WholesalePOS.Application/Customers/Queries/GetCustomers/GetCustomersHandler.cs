using MediatR;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersHandler
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerListItemDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        return await _customerRepository.GetPagedAsync(
            request,
            cancellationToken);
    }
}
