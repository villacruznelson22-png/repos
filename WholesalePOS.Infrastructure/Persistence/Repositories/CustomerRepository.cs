using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Customers.Queries.GetCustomers;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class CustomerRepository
    : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<Customer?> GetByIdWithAddressesAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(x => x.Addresses)
                .ThenInclude(x => x.Barangay)
                    .ThenInclude(x => x.CityMunicipality)
                        .ThenInclude(x => x.Province!)
                            .ThenInclude(x => x.Region)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<PagedResult<CustomerListItemDto>> GetPagedAsync(
        GetCustomersQuery query,
        CancellationToken cancellationToken)
    {
        var customersQuery = _dbSet
            .AsNoTracking();

        if (query.ActiveOnly)
        {
            customersQuery = customersQuery
                .Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            customersQuery = customersQuery
                .Where(x =>
                    x.Name.Contains(search) ||
                    (x.ContactNumber != null &&
                     x.ContactNumber.Contains(search)));
        }

        var totalCount = await customersQuery.CountAsync(
            cancellationToken);

        var items = await customersQuery
            .OrderBy(x => x.Name)
            .ThenBy(x => x.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CustomerListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                ContactNumber = x.ContactNumber,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerListItemDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}