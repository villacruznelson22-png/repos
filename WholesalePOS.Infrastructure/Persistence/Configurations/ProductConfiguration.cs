using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Barcode)
            .HasConversion(
                barcode => barcode == null ? null : barcode.Value,
                value => value == null ? null : new Barcode(value))
            .HasMaxLength(100);

        builder.Property(p => p.DefaultSellingPrice)
            .HasConversion(
                    money => money.Value,
                    value => new Money(value))
            .HasPrecision(18, 2);

        builder.Property(p => p.SuggestedRetailPrice)
            .HasConversion(
                    money => money.Value,
                    value => new Money(value))
            .HasPrecision(18, 2);

        builder.Property(p => p.Stock)
            .HasConversion(
            stock => stock.Value,
            value => new StockQuantity(value))
            .HasPrecision(18, 3);

        //Rowversion for Concurrency
        builder.Property(p => p.Version)
            .IsRowVersion();


        builder.HasIndex(p => p.Barcode)
            .IsUnique();


    }
}