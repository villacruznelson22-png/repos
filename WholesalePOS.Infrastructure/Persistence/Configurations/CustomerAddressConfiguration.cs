using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class CustomerAddressConfiguration
    : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Street)
            .HasMaxLength(300);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20);

        builder.Property(a => a.Landmark)
            .HasMaxLength(300);

        builder.Property(a => a.Latitude)
            .HasPrecision(9, 6);

        builder.Property(a => a.Longitude)
            .HasPrecision(9, 6);

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.HasOne(a => a.Barangay)
            .WithMany()
            .HasForeignKey(a => a.BarangayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CustomerId);

        builder.HasIndex(a => a.BarangayId);
    }
}