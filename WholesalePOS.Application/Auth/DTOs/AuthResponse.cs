namespace WholesalePOS.Application.Auth.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
    public UserSummaryDto User { get; init; } = null!;
}

public sealed class UserSummaryDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}
