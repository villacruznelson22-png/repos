using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Sales.Queries.GetSales;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;

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

    public async Task<PagedResult<SaleListItemDto>> GetPagedAsync(
        GetSalesQuery query,
        CancellationToken cancellationToken)
    {
        var sales = _context.Sales
            .AsNoTracking();

        if (query.OpenOnly)
        {
            sales = sales.Where(x =>
                x.Status == SaleStatus.Draft ||
                x.Status == SaleStatus.Confirmed);
        }
        else if (query.Status.HasValue)
        {
            sales = sales.Where(x =>
                x.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            sales = sales.Where(x =>
                (x.ReferenceNumber != null &&
                 x.ReferenceNumber.Contains(search)) ||
                (x.Customer != null &&
                 x.Customer.Name.Contains(search)));
        }

        var totalCount = await sales.CountAsync(
            cancellationToken);

        var items = await sales
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new SaleListItemDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer == null
                    ? null
                    : x.Customer.Name,
                OccurredAt = x.OccurredAt,
                ReferenceNumber = x.ReferenceNumber,
                Status = x.Status,
                TotalAmount = x.Lines
                    .Sum(line =>
                        line.UnitSellingPrice.Value *
                        line.Quantity)
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SaleListItemDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}