using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IStockMovementRepository
{
    Task AddAsync(
        StockMovement movement,
        CancellationToken cancellationToken);
}