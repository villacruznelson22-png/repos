using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class BarangayConfiguration
    : IEntityTypeConfiguration<Barangay>
{
    public void Configure(EntityTypeBuilder<Barangay> builder)
    {
        builder.ToTable("Barangays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(x => x.CityMunicipality)
            .WithMany(x => x.Barangays)
            .HasForeignKey(x => x.CityMunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}