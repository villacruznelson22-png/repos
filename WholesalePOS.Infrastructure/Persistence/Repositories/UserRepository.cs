using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Users.DTOs;
using WholesalePOS.Application.Users.Queries.GetUsers;
using WholesalePOS.Domain.Common;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly WholesalePosDbContext _context;
    public UserRepository(WholesalePosDbContext context) => _context = context;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        => _context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = _context.Users.AsNoTracking().AsQueryable();
        if (query.IsActive.HasValue)
            usersQuery = usersQuery.Where(x => x.IsActive == query.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            usersQuery = usersQuery.Where(x => x.Username.Contains(search) || x.DisplayName.Contains(search));
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var activeAdminCount = await CountActiveAdminsAsync(cancellationToken);
        var users = await usersQuery.Include(x => x.Roles)
            .OrderBy(x => x.Username).ThenBy(x => x.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>
        {
            Items = users.Select(x =>
            {
                var isLastActiveAdmin = x.IsActive && x.Roles.Any(r => r.Name == RoleNames.Admin) && activeAdminCount <= 1;
                return new UserListItemDto
                {
                    Id = x.Id, Username = x.Username, DisplayName = x.DisplayName,
                    IsActive = x.IsActive, CreatedAt = x.CreatedAt, LastLoginAt = x.LastLoginAt,
                    Roles = x.Roles.Select(r => r.Name).OrderBy(n => n).ToArray(),
                    CanDeactivate = !isLastActiveAdmin,
                    CanRemoveAdminRole = !isLastActiveAdmin
                };
            }).ToArray(),
            PageNumber = query.PageNumber, PageSize = query.PageSize, TotalCount = totalCount
        };
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string hash, CancellationToken cancellationToken)
        => _context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

    public async Task<bool> RevokeRefreshTokenAsync(Guid refreshTokenId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var affectedRows = await _context.RefreshTokens
            .Where(x => x.Id == refreshTokenId && x.RevokedAt == null && x.ExpiresAt > utcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, utcNow), cancellationToken);
        return affectedRows == 1;
    }

    public Task<int> RevokeAllRefreshTokensForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken)
        => _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, utcNow), cancellationToken);

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken)
        => _context.Users.CountAsync(
            user => user.IsActive && user.Roles.Any(role => role.Name == RoleNames.Admin),
            cancellationToken);

    public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken)
        => _context.Roles.SingleOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken)
        => await _context.RefreshTokens.AddAsync(token, cancellationToken);
}