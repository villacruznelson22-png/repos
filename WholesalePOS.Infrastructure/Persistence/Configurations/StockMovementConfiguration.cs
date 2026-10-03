using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class InventoryTransactionConfiguration
    : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Direction)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(
                quantity => quantity == null ? null : quantity.Value,
                value => value == null
                    ? null
                    : new InventoryTransactionQuantity(value))
            .HasPrecision(18, 3)
            .IsRequired(false);

        builder.Property(x => x.UnitCost)
            .HasConversion(
                cost => cost == null ? null : cost.Value,
                value => value == null
                    ? null
                    : new InventoryCost(value))
            .HasPrecision(19, 6)
            .IsRequired(false);

        builder.Property(x => x.TotalCost)
            .HasPrecision(19, 6)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.ReferenceId)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ProductId, x.OccurredAt });

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
