using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IPurchaseOrderRepository
    : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken);
}