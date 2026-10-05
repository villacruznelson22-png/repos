using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SaleLine.
/// </summary>
public class SaleLineConfiguration
    : IEntityTypeConfiguration<SaleLine>
{
    public void Configure(EntityTypeBuilder<SaleLine> builder)
    {
        builder.ToTable("SaleLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SaleId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        // ---------------------------------------------------------
        // Historical inventory cost
        // ---------------------------------------------------------
        //
        // UnitCost is captured only when the sale is completed.
        // Therefore it is nullable while the sale is Draft or
        // Confirmed.
        //
        // InventoryCost uses 6 decimal places because inventory
        // costing requires greater precision than selling prices.
        //
        builder.Property(x => x.UnitCost)
            .HasConversion(
                cost => cost == null
                    ? (decimal?)null
                    : cost.Value,
                value => value.HasValue
                    ? new InventoryCost(value.Value)
                    : null)
            .HasPrecision(19, 6)
            .IsRequired(false);

        // ---------------------------------------------------------
        // Historical selling price
        // ---------------------------------------------------------
        //
        // Selling prices use Money and therefore 2 decimal places.
        //
        builder.Property(x => x.UnitSellingPrice)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // A product may appear only once within a sale.
        // The domain layer also enforces this rule, while this
        // unique index protects the invariant at database level.
        builder.HasIndex(x => new
        {
            x.SaleId,
            x.ProductId
        })
        .IsUnique()
        .HasDatabaseName("UX_SaleLines_Sale_Product");
    }
}