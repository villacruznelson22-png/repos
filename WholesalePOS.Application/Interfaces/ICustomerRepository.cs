using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdWithAddressesAsync(
        Guid id,
        CancellationToken cancellationToken);
}