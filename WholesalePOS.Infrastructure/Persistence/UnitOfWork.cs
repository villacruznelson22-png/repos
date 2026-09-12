using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly WholesalePosDbContext _context;

    public UnitOfWork(
        WholesalePosDbContext context)
    {
        _context = context;
    }
        
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "The record was modified by another user. Please reload the data and try again.");
        }
    }
}