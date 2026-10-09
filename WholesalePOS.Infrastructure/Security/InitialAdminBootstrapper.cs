using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Common;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Security;

public sealed class InitialAdminBootstrapper : IInitialAdminBootstrapper
{
    private readonly WholesalePosDbContext _db;
    private readonly IPasswordService _passwordService;

    public InitialAdminBootstrapper(
        WholesalePosDbContext db,
        IPasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    public async Task EnsureInitialAdminAsync(
        string? username,
        string? displayName,
        string? password,
        CancellationToken cancellationToken)
    {
        // Never bootstrap an additional privileged account into a database
        // that already contains users.
        if (await _db.Users.AnyAsync(cancellationToken))
            return;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "No users exist. Configure BootstrapAdmin:Username, " +
                "BootstrapAdmin:DisplayName, and BootstrapAdmin:Password " +
                "in Development to create the initial administrator.");
        }

        if (password.Length < 12 || password.Length > 128)
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Password must be between 12 and 128 characters.");
        }

        var adminRole = await _db.Roles.SingleOrDefaultAsync(
            role => role.Name == RoleNames.Admin,
            cancellationToken);

        if (adminRole is null)
        {
            throw new InvalidOperationException(
                "The Admin role was not found. Apply database migrations first.");
        }

        var admin = new User(
            username.Trim(),
            displayName.Trim(),
            _passwordService.Hash(password));

        admin.AssignRole(adminRole);
        _db.Users.Add(admin);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
