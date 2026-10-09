using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence;

public class WholesalePosDbContext : DbContext
{
    public WholesalePosDbContext(DbContextOptions<WholesalePosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();

    public DbSet<InventoryTransaction> InventoryTransactions =>
        Set<InventoryTransaction>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    #region Customer Address

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<CityMunicipality> CityMunicipalities => Set<CityMunicipality>();
    public DbSet<Barangay> Barangays => Set<Barangay>();

    #endregion

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<DeliveryReceipt> DeliveryReceipts => Set<DeliveryReceipt>();
    public DbSet<DeliveryReceiptLine> DeliveryReceiptLines => Set<DeliveryReceiptLine>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<SaleDiscount> SaleDiscounts => Set<SaleDiscount>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WholesalePosDbContext).Assembly);
    }
}
