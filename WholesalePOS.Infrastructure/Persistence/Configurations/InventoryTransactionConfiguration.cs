using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database representation of the inventory audit ledger.
/// </summary>
public class InventoryTransactionConfiguration
    : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Direction).IsRequired();

        // Nullable because CostCorrection changes value without changing quantity.
        builder.Property(x => x.Quantity)
            .HasConversion(
                new ValueConverter<InventoryTransactionQuantity?, decimal?>(
                    quantity => quantity == null ? null : quantity.Value,
                    value => value == null
                        ? null
                        : new InventoryTransactionQuantity(value.Value)))
            .HasPrecision(18, 3)
            .IsRequired(false);

        // Nullable because CostCorrection is a value-only transaction.
        builder.Property(x => x.UnitCost)
            .HasConversion(
                new ValueConverter<InventoryCost?, decimal?>(
                    cost => cost == null ? null : cost.Value,
                    value => value == null
                        ? null
                        : new InventoryCost(value.Value)))
            .HasPrecision(19, 6)
            .IsRequired(false);

        builder.Property(x => x.TotalCost)
            .HasPrecision(19, 6)
            .IsRequired();

        builder.Property(x => x.OccurredAt).IsRequired();

        // ReferenceType + ReferenceId provide traceability to the source document.
        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.ReferenceId)
            .IsRequired(false);

        // Supports stock-card queries ordered by product and event time.
        builder.HasIndex(x => new { x.ProductId, x.OccurredAt });

        // Inventory history must not disappear when a Product is deleted.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
