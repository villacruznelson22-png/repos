using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class DeliveryReceiptLineConfiguration
    : IEntityTypeConfiguration<DeliveryReceiptLine>
{
    public void Configure(EntityTypeBuilder<DeliveryReceiptLine> builder)
    {
        builder.ToTable("DeliveryReceiptLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeliveryReceiptId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(x => x.UnitCost)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ExpirationDate);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeliveryReceipt)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.DeliveryReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}