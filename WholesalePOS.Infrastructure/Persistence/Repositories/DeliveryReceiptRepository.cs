using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class DeliveryReceiptRepository
    : Repository<DeliveryReceipt>,
      IDeliveryReceiptRepository
{
   
    public DeliveryReceiptRepository(
        WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<DeliveryReceipt?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.DeliveryReceipts
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.Supplier)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}