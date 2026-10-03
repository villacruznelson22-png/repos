using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation for the inventory transaction audit ledger.
/// </summary>
public class InventoryTransactionRepository
    : Repository<InventoryTransaction>,
      IInventoryTransactionRepository
{
    public InventoryTransactionRepository(WholesalePosDbContext context)
        : base(context)
    {
    }
}
