using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly WholesalePosDbContext _context;

    public UserRepository(WholesalePosDbContext context)
        => _context = context;

    public Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
        => _context.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
        => _context.Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<RefreshToken?> GetRefreshTokenAsync(
        string hash,
        CancellationToken cancellationToken)
        => _context.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

    public async Task<bool> RevokeRefreshTokenAsync(
        Guid refreshTokenId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _context.RefreshTokens
            .Where(x =>
                x.Id == refreshTokenId &&
                x.RevokedAt == null &&
                x.ExpiresAt > utcNow)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.RevokedAt,
                    utcNow),
                cancellationToken);

        return affectedRows == 1;
    }

    public Task<Role?> GetRoleByNameAsync(
        string name,
        CancellationToken cancellationToken)
        => _context.Roles
            .SingleOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);

    public async Task AddRefreshTokenAsync(
        RefreshToken token,
        CancellationToken cancellationToken)
        => await _context.RefreshTokens.AddAsync(token, cancellationToken);
}
