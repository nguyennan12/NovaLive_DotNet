using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.FlashSales;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class FlashSaleConfiguration :
    IEntityTypeConfiguration<FlashSaleCampaign>,
    IEntityTypeConfiguration<FlashSaleItem>
{
    public void Configure(EntityTypeBuilder<FlashSaleCampaign> builder)
    {
        builder.ToTable("flash_sale_campaigns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.BannerUrl).HasMaxLength(500);

        builder.HasIndex(c => new { c.Status, c.StartAt, c.EndAt });
    }

    public void Configure(EntityTypeBuilder<FlashSaleItem> builder)
    {
        builder.ToTable("flash_sale_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.FlashPrice).HasPrecision(18, 2);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(item => new { item.CampaignId, item.SkuId }).IsUnique();
        builder.HasIndex(item => new { item.CampaignId, item.Status });
        builder.HasIndex(item => item.SkuId);
    }
}
