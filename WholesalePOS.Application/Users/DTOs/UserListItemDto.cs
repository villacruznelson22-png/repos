namespace WholesalePOS.Application.Users.DTOs;

public sealed class UserListItemDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool CanDeactivate { get; init; } = true;
    public bool CanRemoveAdminRole { get; init; } = true;
}