using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class RegionConfiguration
    : IEntityTypeConfiguration<Region>
{
    public void Configure(
        EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasMany(x => x.Provinces)
            .WithOne(x => x.Region)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CityMunicipalities)
            .WithOne(x => x.Region)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}