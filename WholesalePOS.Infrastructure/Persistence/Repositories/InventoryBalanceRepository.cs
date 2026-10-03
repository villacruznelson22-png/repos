using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class InventoryBalanceRepository
    : Repository<InventoryBalance>, IInventoryBalanceRepository
{
    public InventoryBalanceRepository(WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<InventoryBalance?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<InventoryBalance>()
            .SingleOrDefaultAsync(
                x => x.ProductId == productId,
                cancellationToken);
    }
}
