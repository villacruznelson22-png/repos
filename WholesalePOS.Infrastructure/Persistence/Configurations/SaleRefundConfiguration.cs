using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public sealed class SaleRefundConfiguration : IEntityTypeConfiguration<SaleRefund>
{
    public void Configure(EntityTypeBuilder<SaleRefund> builder)
    {
        builder.ToTable("SaleRefunds");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SaleId).IsRequired();
        builder.Property(x => x.Amount)
            .HasConversion(amount => amount.Value, value => new Money(value))
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(x => x.Method).HasConversion<int>().IsRequired();
        builder.Property(x => x.RefundedAt).IsRequired();
        builder.Property(x => x.RefundedByUserId).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_SaleRefunds_IdempotencyKey");

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RefundedByUser)
            .WithMany()
            .HasForeignKey(x => x.RefundedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
