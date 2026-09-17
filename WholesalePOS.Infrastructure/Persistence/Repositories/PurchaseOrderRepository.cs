using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository
    : Repository<PurchaseOrder>,
      IPurchaseOrderRepository
{

    public PurchaseOrderRepository(
        WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<PurchaseOrder?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}