using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public class ProvinceConfiguration
    : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(x => x.Region)
            .WithMany(x => x.Provinces)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CityMunicipalities)
            .WithOne(x => x.Province)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}