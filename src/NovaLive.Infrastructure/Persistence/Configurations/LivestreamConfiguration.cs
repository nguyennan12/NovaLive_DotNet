using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class LivestreamConfiguration :
    IEntityTypeConfiguration<LivestreamSession>,
    IEntityTypeConfiguration<LivestreamProduct>,
    IEntityTypeConfiguration<LivestreamComment>
{
    public void Configure(EntityTypeBuilder<LivestreamSession> builder)
    {
        builder.ToTable("livestream_sessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).HasMaxLength(300).IsRequired();
        builder.Property(s => s.ThumbnailUrl).HasMaxLength(500);
        builder.Property(s => s.AgoraChannelName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.PlaybackUrl).HasMaxLength(500);

        builder.HasIndex(s => s.AgoraChannelName).IsUnique();
        builder.HasIndex(s => new { s.ShopId, s.Status });
    }

    public void Configure(EntityTypeBuilder<LivestreamProduct> builder)
    {
        builder.ToTable("livestream_products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FlashPrice).HasPrecision(18, 2);

        builder.HasIndex(p => new { p.SessionId, p.SkuId }).IsUnique();
        builder.HasIndex(p => new { p.SessionId, p.DisplayOrder });
    }

    public void Configure(EntityTypeBuilder<LivestreamComment> builder)
    {
        builder.ToTable("livestream_comments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Content).HasMaxLength(500).IsRequired();

        builder.HasIndex(c => new { c.SessionId, c.CreatedAt });
    }
}
