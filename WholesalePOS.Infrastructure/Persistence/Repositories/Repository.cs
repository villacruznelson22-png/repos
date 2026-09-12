using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly WholesalePosDbContext _context;

    protected readonly DbSet<T> _dbSet;

    public Repository(WholesalePosDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(
            new object[] { id },
            cancellationToken);
    }

    public virtual async Task AddAsync(
        T entity,
        CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

  
}