using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesalePOS.Domain.Common;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly Guid AdminId = new("8F4D9B8A-1D1B-4F90-8A6A-000000000001");
    private static readonly Guid ManagerId = new("8F4D9B8A-1D1B-4F90-8A6A-000000000002");
    private static readonly Guid CashierId = new("8F4D9B8A-1D1B-4F90-8A6A-000000000003");
    private static readonly Guid InventoryClerkId = new("8F4D9B8A-1D1B-4F90-8A6A-000000000004");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new { Id = AdminId, Name = RoleNames.Admin },
            new { Id = ManagerId, Name = RoleNames.Manager },
            new { Id = CashierId, Name = RoleNames.Cashier },
            new { Id = InventoryClerkId, Name = RoleNames.InventoryClerk });
    }
}
