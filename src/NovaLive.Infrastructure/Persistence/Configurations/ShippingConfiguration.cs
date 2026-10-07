using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Shipping;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class ShippingConfiguration : IEntityTypeConfiguration<ShippingOrder>
{
    public void Configure(EntityTypeBuilder<ShippingOrder> builder)
    {
        builder.ToTable("shipping_orders");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Provider).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ServiceCode).HasMaxLength(50).IsRequired();
        builder.Property(s => s.TrackingCode).HasMaxLength(100);
        builder.Property(s => s.ProviderOrderId).HasMaxLength(100);
        builder.Property(s => s.PickupAddressJson).HasColumnType("jsonb");
        builder.Property(s => s.DeliveryAddressJson).HasColumnType("jsonb");
        builder.Property(s => s.CodAmount).HasPrecision(18, 2);
        builder.Property(s => s.ShippingFee).HasPrecision(18, 2);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.WebhookPayload).HasColumnType("jsonb");

        builder.HasIndex(s => s.SubOrderId).IsUnique();
        builder.HasIndex(s => new { s.Provider, s.TrackingCode }).HasFilter("tracking_code IS NOT NULL");
        builder.HasIndex(s => new { s.Status, s.UpdatedAt });
    }
}
