using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

/// <summary>
/// Persistence abstraction for the current inventory balance of a product.
/// </summary>
public interface IInventoryBalanceRepository : IRepository<InventoryBalance>
{
    /// <summary>
    /// Finds the single current inventory balance associated with a product.
    /// </summary>
    Task<InventoryBalance?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken);
}
