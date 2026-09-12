using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Repositories;

public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{

    public StockMovementRepository(WholesalePosDbContext context) : base(context)
    {
    }
}