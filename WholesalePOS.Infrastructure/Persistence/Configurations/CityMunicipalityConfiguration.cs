using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class CityMunicipalityConfiguration
    : IEntityTypeConfiguration<CityMunicipality>
{
    public void Configure(
        EntityTypeBuilder<CityMunicipality> builder)
    {
        builder.ToTable("CityMunicipalities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(x => x.Region)
            .WithMany(x => x.CityMunicipalities)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Province)
            .WithMany(x => x.CityMunicipalities)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Barangays)
            .WithOne(x => x.CityMunicipality)
            .HasForeignKey(x => x.CityMunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}