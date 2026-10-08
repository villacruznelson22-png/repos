using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public byte[] Version { get; private set; } = null!;

    private User() { }

    public User(string username, string displayName, string passwordHash)
    {
        Id = Guid.NewGuid();
        ChangeUsername(username);
        ChangeDisplayName(displayName);
        SetPasswordHash(passwordHash);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new UserDomainException("Username cannot be empty.");

        Username = username.Trim();
    }

    public void ChangeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDomainException("Display name cannot be empty.");

        DisplayName = displayName.Trim();
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new UserDomainException("Password hash cannot be empty.");

        PasswordHash = passwordHash;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
}