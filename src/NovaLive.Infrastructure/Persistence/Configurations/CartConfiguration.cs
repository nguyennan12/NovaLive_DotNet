using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Carts;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration :
    IEntityTypeConfiguration<Cart>,
    IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.UserId).IsUnique();
    }

    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);

        builder.HasIndex(item => new { item.CartId, item.SkuId }).IsUnique();
        builder.HasIndex(item => new { item.CartId, item.ShopId });
    }
}
