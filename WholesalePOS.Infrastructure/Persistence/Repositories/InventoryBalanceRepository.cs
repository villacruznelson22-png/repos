using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation for current inventory balances.
/// </summary>
public class InventoryBalanceRepository
    : Repository<InventoryBalance>, IInventoryBalanceRepository
{
    public InventoryBalanceRepository(WholesalePosDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Retrieves the current balance for a product.
    /// The database enforces uniqueness so at most one balance can exist.
    /// </summary>
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
