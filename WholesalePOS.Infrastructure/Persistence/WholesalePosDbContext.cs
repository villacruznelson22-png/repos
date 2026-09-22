using Microsoft.EntityFrameworkCore;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence;

public class WholesalePosDbContext : DbContext
{
    public WholesalePosDbContext(DbContextOptions<WholesalePosDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();


    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //This tells EF:
        //"Go through this assembly and automatically find every class implementing IEntityTypeConfiguration<T>."

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WholesalePosDbContext).Assembly);

    }
}