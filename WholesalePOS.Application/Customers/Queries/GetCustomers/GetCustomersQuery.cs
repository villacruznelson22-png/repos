using MediatR;
using WholesalePOS.Application.Common.Models;

namespace WholesalePOS.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersQuery
    : PaginationRequest,
      IRequest<PagedResult<CustomerListItemDto>>
{
    public string? Search { get; init; }

    public bool ActiveOnly { get; init; }
}
