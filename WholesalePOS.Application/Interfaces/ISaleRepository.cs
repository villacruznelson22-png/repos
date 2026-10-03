using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken);
}