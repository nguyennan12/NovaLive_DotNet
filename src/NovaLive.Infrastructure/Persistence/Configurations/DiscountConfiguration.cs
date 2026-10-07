using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Discounts;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class DiscountConfiguration :
    IEntityTypeConfiguration<Discount>,
    IEntityTypeConfiguration<DiscountUsage>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("discounts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.DiscountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.DiscountValue).HasPrecision(18, 2);
        builder.Property(d => d.MinOrderAmount).HasPrecision(18, 2);
        builder.Property(d => d.MaxDiscountAmount).HasPrecision(18, 2);
        builder.Property(d => d.AppliesTo).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(d => d.Code).IsUnique();
        builder.HasIndex(d => new { d.ShopId, d.IsActive, d.ValidFrom, d.ValidTo }).HasFilter("is_active = TRUE");
    }

    public void Configure(EntityTypeBuilder<DiscountUsage> builder)
    {
        builder.ToTable("discount_usages");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.DiscountAmount).HasPrecision(18, 2);

        builder.HasIndex(u => new { u.DiscountId, u.ParentOrderId }).IsUnique();
        builder.HasIndex(u => new { u.UserId, u.DiscountId });
    }
}
