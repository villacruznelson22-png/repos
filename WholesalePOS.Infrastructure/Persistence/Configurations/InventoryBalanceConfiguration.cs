using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class InventoryBalanceConfiguration
    : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasIndex(x => x.ProductId)
            .IsUnique();

        builder.Property(x => x.QuantityOnHand)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(x => x.InventoryValue)
            .HasPrecision(19, 6)
            .IsRequired();

        builder.Property(x => x.AverageUnitCost)
            .HasConversion(
                cost => cost.Value,
                value => new InventoryCost(value))
            .HasPrecision(19, 6)
            .IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
