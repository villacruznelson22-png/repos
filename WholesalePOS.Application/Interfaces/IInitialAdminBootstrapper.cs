namespace WholesalePOS.Application.Interfaces;

public interface IInitialAdminBootstrapper
{
    Task EnsureInitialAdminAsync(
        string? username,
        string? displayName,
        string? password,
        CancellationToken cancellationToken);
}
