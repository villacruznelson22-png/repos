using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IDeliveryReceiptRepository
    : IRepository<DeliveryReceipt>
{
    Task<DeliveryReceipt?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken);
}