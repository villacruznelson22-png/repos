using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Sales.Queries.GetSales;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResult<SaleListItemDto>> GetPagedAsync(
        GetSalesQuery query,
        CancellationToken cancellationToken);
}