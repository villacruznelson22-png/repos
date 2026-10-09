using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Infrastructure.Security;
using WholesalePOS.Domain.Services;
using WholesalePOS.Infrastructure.Imports.Psgc;
using WholesalePOS.Infrastructure.Persistence;
using WholesalePOS.Infrastructure.Persistence.Repositories;

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
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IInventoryBalanceRepository, InventoryBalanceRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseOrderRepository,PurchaseOrderRepository>();
        services.AddScoped<IDeliveryReceiptRepository, DeliveryReceiptRepository>();

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IInitialAdminBootstrapper, InitialAdminBootstrapper>();

        services.AddScoped<InventoryService>();
        services.AddScoped<PsgcImportService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}