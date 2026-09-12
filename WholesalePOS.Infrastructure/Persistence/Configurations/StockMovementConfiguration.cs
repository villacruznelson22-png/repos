using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration: IEntityTypeConfiguration<StockMovement>
{
    public void Configure(
        EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Direction)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(
                quantity => quantity.Value,
                value => new StockMovementQuantity(value))
            .HasPrecision(18, 3);

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                }
}