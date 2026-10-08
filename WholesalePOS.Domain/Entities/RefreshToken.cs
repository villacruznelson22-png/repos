using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new UserDomainException("User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new UserDomainException("Refresh token hash cannot be empty.");

        if (expiresAt <= DateTime.UtcNow)
            throw new UserDomainException("Refresh token must expire in the future.");

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash.Trim();
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsActive(DateTime utcNow)
        => RevokedAt is null && ExpiresAt > utcNow;

    public void Revoke()
    {
        if (RevokedAt is null)
            RevokedAt = DateTime.UtcNow;
    }
}