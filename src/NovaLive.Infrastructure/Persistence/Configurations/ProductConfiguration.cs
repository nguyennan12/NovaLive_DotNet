using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration :
    IEntityTypeConfiguration<Category>,
    IEntityTypeConfiguration<Spu>,
    IEntityTypeConfiguration<Sku>,
    IEntityTypeConfiguration<SkuImage>,
    IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name).HasMaxLength(200).IsRequired();
        builder.Property(category => category.Slug).HasMaxLength(200).IsRequired();
        builder.Property(category => category.IconUrl).HasMaxLength(500);

        builder.HasIndex(category => category.Slug).IsUnique();
        builder.HasIndex(category => category.ParentId);
    }

    public void Configure(EntityTypeBuilder<Spu> builder)
    {
        builder.ToTable("spus");
        builder.HasKey(spu => spu.Id);

        builder.Property(spu => spu.Name).HasMaxLength(300).IsRequired();
        builder.Property(spu => spu.Brand).HasMaxLength(150);
        builder.Property(spu => spu.ThumbnailUrl).HasMaxLength(500);
        builder.Property(spu => spu.AttributesConfigJson).HasColumnType("jsonb");
        builder.Property(spu => spu.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(spu => new { spu.ShopId, spu.Status }).HasFilter("deleted_at IS NULL");
        builder.HasIndex(spu => spu.CategoryId).HasFilter("deleted_at IS NULL");
    }

    public void Configure(EntityTypeBuilder<Sku> builder)
    {
        builder.ToTable("skus");
        builder.HasKey(sku => sku.Id);

        builder.Property(sku => sku.SkuCode).HasMaxLength(100).IsRequired();
        builder.Property(sku => sku.AttributesJson).HasColumnType("jsonb");
        builder.Property(sku => sku.OriginalPrice).HasPrecision(18, 2);
        builder.Property(sku => sku.SellPrice).HasPrecision(18, 2);

        builder.HasIndex(sku => new { sku.ShopId, sku.SkuCode }).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(sku => sku.SpuId).HasFilter("deleted_at IS NULL");
        builder.HasIndex(sku => new { sku.ShopId, sku.IsActive }).HasFilter("deleted_at IS NULL");
    }

    public void Configure(EntityTypeBuilder<SkuImage> builder)
    {
        builder.ToTable("sku_images");
        builder.HasKey(img => img.Id);

        builder.Property(img => img.ImageUrl).HasMaxLength(500).IsRequired();

        builder.HasIndex(img => img.SkuId);
    }

    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");
        builder.HasKey(attr => attr.Id);

        builder.Property(attr => attr.AttrName).HasMaxLength(100).IsRequired();
        builder.Property(attr => attr.AttrValue).HasMaxLength(300).IsRequired();

        builder.HasIndex(attr => attr.SpuId);
    }
}
