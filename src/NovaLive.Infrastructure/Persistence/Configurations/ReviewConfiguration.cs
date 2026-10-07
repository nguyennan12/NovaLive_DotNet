using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Reviews;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration :
    IEntityTypeConfiguration<Review>,
    IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(200);

        builder.HasIndex(r => r.OrderItemId).IsUnique();
        builder.HasIndex(r => new { r.SkuId, r.Rating }).HasFilter("is_visible = TRUE");
        builder.HasIndex(r => new { r.ShopId, r.Rating }).HasFilter("is_visible = TRUE");
    }

    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.ToTable("review_images");
        builder.HasKey(img => img.Id);

        builder.Property(img => img.MediaUrl).HasMaxLength(500).IsRequired();
        builder.Property(img => img.MediaType).HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(img => img.ReviewId);
    }
}
