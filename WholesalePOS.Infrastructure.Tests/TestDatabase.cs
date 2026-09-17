using Microsoft.EntityFrameworkCore;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests;

public static class TestDatabase
{
    private const string ConnectionString =
        "Server=localhost\\SQLEXPRESS;" +
        "Database=WholesalePOSDb_Test;" +
        "Trusted_Connection=True;" +
        "TrustServerCertificate=True;";

    public static async Task ResetAsync()
    {
        var options =
            new DbContextOptionsBuilder<WholesalePosDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

        await using var context =
            new WholesalePosDbContext(options);

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}