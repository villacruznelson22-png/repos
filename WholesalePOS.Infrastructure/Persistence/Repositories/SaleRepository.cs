using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class SaleRepository
    : Repository<Sale>,
      ISaleRepository
{
    public SaleRepository(
        WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<Sale?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Include(x => x.Customer)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}