using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IInventoryTransactionRepository
{
    Task AddAsync(
        InventoryTransaction transaction,
        CancellationToken cancellationToken);
}
