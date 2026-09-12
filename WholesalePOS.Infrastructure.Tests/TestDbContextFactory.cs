using Microsoft.EntityFrameworkCore;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Tests;

public class TestDbContextFactory
{
    private readonly DbContextOptions<WholesalePosDbContext> _options;

    public TestDbContextFactory(string connectionString)
    {
        _options =
            new DbContextOptionsBuilder<WholesalePosDbContext>()
                .UseSqlServer(connectionString)
                .Options;
    }

    public WholesalePosDbContext Create()
    {
        return new WholesalePosDbContext(_options);
    }
}