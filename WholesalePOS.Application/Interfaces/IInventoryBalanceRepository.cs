using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IInventoryBalanceRepository : IRepository<InventoryBalance>
{
    Task<InventoryBalance?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken);
}
