using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class SaleDiscountConfiguration : IEntityTypeConfiguration<SaleDiscount>
{
    public void Configure(EntityTypeBuilder<SaleDiscount> builder)
    {
        builder.ToTable("SaleDiscounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Scope)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CalculationType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.AppliedAt)
            .IsRequired();

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Discounts)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SaleLine)
            .WithMany()
            .HasForeignKey(x => x.SaleLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AppliedByUser)
            .WithMany()
            .HasForeignKey(x => x.AppliedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SaleId);
        builder.HasIndex(x => x.SaleLineId);
        builder.HasIndex(x => x.AppliedByUserId);
    }
}
