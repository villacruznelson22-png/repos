using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Services;
using WholesalePOS.Infrastructure.Imports.Psgc;
using WholesalePOS.Infrastructure.Persistence;
using WholesalePOS.Infrastructure.Persistence.Repositories;
using WholesalePOS.Infrastructure.Repositories;

namespace WholesalePOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WholesalePosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("WholesalePOS")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        services.AddScoped<InventoryService>();
        services.AddScoped<PsgcImportService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();


        return services;
    }
}