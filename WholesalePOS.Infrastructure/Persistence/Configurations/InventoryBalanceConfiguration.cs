using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines how <see cref="InventoryBalance"/> is persisted.
/// Database constraints here protect the same invariants that are important
/// to the domain model at the storage boundary.
/// </summary>
public class InventoryBalanceConfiguration
    : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId).IsRequired();

        // BUSINESS RULE / DATA INTEGRITY:
        // There must be exactly one current inventory balance per product.
        builder.HasIndex(x => x.ProductId).IsUnique();

        builder.Property(x => x.QuantityOnHand)
            .HasPrecision(18, 3)
            .IsRequired();

        // Six decimal places are retained internally for inventory valuation.
        builder.Property(x => x.InventoryValue)
            .HasPrecision(19, 6)
            .IsRequired();

        // InventoryCost is a domain value object, but SQL stores its decimal value.
        builder.Property(x => x.AverageUnitCost)
            .HasConversion(
                new ValueConverter<InventoryCost, decimal>(
                    cost => cost.Value,
                    value => new InventoryCost(value)))
            .HasPrecision(19, 6)
            .IsRequired();

        // Prevent deleting a Product while inventory state still references it.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
