using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly WholesalePosDbContext _context;
    public UserRepository(WholesalePosDbContext context) => _context = context;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => _context.Users.SingleOrDefaultAsync(x => x.Username == username, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => _context.Users.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<RefreshToken?> GetRefreshTokenAsync(string hash, CancellationToken ct)
        => _context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);

    public async Task<bool> RevokeRefreshTokenAsync(
        Guid refreshTokenId,
        DateTime utcNow,
        CancellationToken ct)
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
                ct);

        return affectedRows == 1;
    }

    public async Task AddAsync(User user, CancellationToken ct)
        => await _context.Users.AddAsync(user, ct);

    public async Task AddRefreshTokenAsync(
        RefreshToken token,
        CancellationToken ct)
        => await _context.RefreshTokens.AddAsync(token, ct);
}