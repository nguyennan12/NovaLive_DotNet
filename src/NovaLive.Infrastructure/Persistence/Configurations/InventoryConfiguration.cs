using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Inventory;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration :
    IEntityTypeConfiguration<Inventory>,
    IEntityTypeConfiguration<InventoryHistory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("inventories");
        builder.HasKey(inv => inv.Id);

        builder.HasIndex(inv => inv.SkuId).IsUnique();
        builder.HasIndex(inv => inv.ShopId);
    }

    public void Configure(EntityTypeBuilder<InventoryHistory> builder)
    {
        builder.ToTable("inventory_histories");
        builder.HasKey(hist => hist.Id);

        builder.Property(hist => hist.ChangeType).HasConversion<string>().HasMaxLength(30);
        builder.Property(hist => hist.RefType).HasMaxLength(30);

        builder.HasIndex(hist => new { hist.SkuId, hist.CreatedAt });
        builder.HasIndex(hist => new { hist.RefType, hist.RefId });
    }
}
