using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLive.Domain.Orders;

namespace NovaLive.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration :
    IEntityTypeConfiguration<ParentOrder>,
    IEntityTypeConfiguration<SubOrder>,
    IEntityTypeConfiguration<OrderItem>,
    IEntityTypeConfiguration<OrderStatusHistory>,
    IEntityTypeConfiguration<OrderReturn>,
    IEntityTypeConfiguration<OrderReturnItem>
{
    public void Configure(EntityTypeBuilder<ParentOrder> builder)
    {
        builder.ToTable("parent_orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderCode).HasMaxLength(50).IsRequired();
        builder.Property(order => order.ShippingAddressJson).HasColumnType("jsonb");
        builder.Property(order => order.TotalItemAmount).HasPrecision(18, 2);
        builder.Property(order => order.TotalShippingFee).HasPrecision(18, 2);
        builder.Property(order => order.TotalDiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.GrandTotal).HasPrecision(18, 2);
        builder.Property(order => order.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(order => order.OrderCode).IsUnique();
        builder.HasIndex(order => new { order.BuyerId, order.CreatedAt });
    }

    public void Configure(EntityTypeBuilder<SubOrder> builder)
    {
        builder.ToTable("sub_orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.SubOrderCode).HasMaxLength(50).IsRequired();
        builder.Property(order => order.OrderSource).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.ItemAmount).HasPrecision(18, 2);
        builder.Property(order => order.ShippingFee).HasPrecision(18, 2);
        builder.Property(order => order.ShopDiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.PlatformDiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.SubTotal).HasPrecision(18, 2);
        builder.Property(order => order.SellerEarnings).HasPrecision(18, 2);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(order => order.SubOrderCode).IsUnique();
        builder.HasIndex(order => order.ParentOrderId);
        builder.HasIndex(order => new { order.ShopId, order.Status });
    }

    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.SkuSnapshotJson).HasColumnType("jsonb");
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.OriginalPrice).HasPrecision(18, 2);
        builder.Property(item => item.DiscountAmount).HasPrecision(18, 2);
        builder.Property(item => item.LineTotal).HasPrecision(18, 2);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(item => item.SubOrderId);
        builder.HasIndex(item => item.SkuId);
    }

    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_histories");
        builder.HasKey(hist => hist.Id);

        builder.Property(hist => hist.FromStatus).HasMaxLength(30);
        builder.Property(hist => hist.ToStatus).HasMaxLength(30).IsRequired();
        builder.Property(hist => hist.ChangedByRole).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(hist => new { hist.SubOrderId, hist.ChangedAt });
    }

    public void Configure(EntityTypeBuilder<OrderReturn> builder)
    {
        builder.ToTable("order_returns");
        builder.HasKey(ret => ret.Id);

        builder.Property(ret => ret.Reason).HasConversion<string>().HasMaxLength(50);
        builder.Property(ret => ret.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(ret => ret.RefundAmount).HasPrecision(18, 2);

        builder.HasIndex(ret => ret.SubOrderId);
        builder.HasIndex(ret => ret.Status).HasFilter("status IN ('Pending','AdminDispute')");
    }

    public void Configure(EntityTypeBuilder<OrderReturnItem> builder)
    {
        builder.ToTable("order_return_items");
        builder.HasKey(item => item.Id);
    }
}
