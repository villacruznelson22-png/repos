using Microsoft.EntityFrameworkCore;
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
}