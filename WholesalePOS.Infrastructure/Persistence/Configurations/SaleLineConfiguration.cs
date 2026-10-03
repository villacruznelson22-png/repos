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

        // Historical inventory cost captured when the sale is created.
        // This is system-controlled and must not be edited by sales users.
        builder.Property(x => x.UnitCost)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasPrecision(18, 2)
            .IsRequired();

        // Historical selling price captured when the sale is created.
        // Authorized users may change this while the sale is still a draft.
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
        // The application/domain layer also enforces this rule,
        // while this index protects the invariant at database level.
        builder.HasIndex(x => new { x.SaleId, x.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_SaleLines_Sale_Product");
    }
}