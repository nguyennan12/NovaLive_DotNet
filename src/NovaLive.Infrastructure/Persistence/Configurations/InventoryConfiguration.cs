using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration :
    IEntityTypeConfiguration<Inventory>,
    IEntityTypeConfiguration<InventoryHistory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("inventories", t =>
        {
            t.HasCheckConstraint("chk_inventory_qty_on_hand_non_negative", "qty_on_hand >= 0");
            t.HasCheckConstraint("chk_inventory_reserved_qty_non_negative", "reserved_qty >= 0");
            t.HasCheckConstraint("chk_inventory_on_hand_gte_reserved", "qty_on_hand >= reserved_qty");
            t.HasCheckConstraint("chk_inventory_min_stock_non_negative", "min_stock >= 0");
        });

        builder.HasKey(inv => inv.Id);

        builder.Property(inv => inv.Version)
            .IsConcurrencyToken();

        builder.HasIndex(inv => inv.SkuId).IsUnique();
        builder.HasIndex(inv => inv.ShopId);

        builder.HasOne<Sku>()
            .WithOne()
            .HasForeignKey<Inventory>(inv => inv.SkuId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<InventoryHistory> builder)
    {
        builder.ToTable("inventory_histories");
        builder.HasKey(hist => hist.Id);

        builder.Property(hist => hist.OperationId).IsRequired();
        builder.Property(hist => hist.ChangeType).HasConversion<string>().HasMaxLength(30);
        builder.Property(hist => hist.RefType).HasMaxLength(50);
        builder.Property(hist => hist.Note).HasMaxLength(500);

        builder.HasIndex(hist => hist.OperationId).IsUnique();
        builder.HasIndex(hist => new { hist.SkuId, hist.CreatedAt });
        builder.HasIndex(hist => new { hist.RefType, hist.RefId });

        builder.HasOne<Inventory>()
            .WithMany()
            .HasForeignKey(hist => hist.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Sku>()
            .WithMany()
            .HasForeignKey(hist => hist.SkuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
