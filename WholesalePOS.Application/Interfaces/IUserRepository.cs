using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Users.DTOs;
using WholesalePOS.Application.Users.Queries.GetUsers;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<UserListItemDto>> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(Guid refreshTokenId, DateTime utcNow, CancellationToken cancellationToken);
    Task<int> RevokeAllRefreshTokensForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken);
    Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}
