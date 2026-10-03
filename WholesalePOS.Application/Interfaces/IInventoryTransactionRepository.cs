using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

/// <summary>
/// Persistence abstraction for the inventory audit ledger.
/// </summary>
public interface IInventoryTransactionRepository
{
    /// <summary>
    /// Adds an inventory transaction to the current unit of work.
    /// The transaction is persisted when <see cref="IUnitOfWork.SaveChangesAsync"/>
    /// is called by the application operation.
    /// </summary>
    Task AddAsync(
        InventoryTransaction transaction,
        CancellationToken cancellationToken);
}
