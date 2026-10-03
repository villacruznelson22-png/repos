using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class InventoryTransactionRepository
    : Repository<InventoryTransaction>,
      IInventoryTransactionRepository
{
    public InventoryTransactionRepository(WholesalePosDbContext context)
        : base(context)
    {
    }
}
