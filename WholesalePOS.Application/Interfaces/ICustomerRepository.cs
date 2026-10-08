using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Customers.Queries.GetCustomers;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdWithAddressesAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResult<CustomerListItemDto>> GetPagedAsync(
        GetCustomersQuery query,
        CancellationToken cancellationToken);
}