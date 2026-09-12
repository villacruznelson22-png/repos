using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Interfaces
{
    public interface IRepository<T>
     where T : class
    {
        Task<T?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task AddAsync(
            T entity,
            CancellationToken cancellationToken);

        void Update(T entity);

        void Remove(T entity);

        
    }
}
