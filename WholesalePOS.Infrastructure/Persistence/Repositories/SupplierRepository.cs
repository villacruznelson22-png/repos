using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class SupplierRepository
    : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(WholesalePosDbContext context)
        : base(context)
    {
    }
}