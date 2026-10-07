using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class SubOrder : AuditableEntity
{
    public Guid ParentOrderId { get; set; }

    public Guid ShopId { get; set; }

    public string SubOrderCode { get; set; } = string.Empty;

    public Guid? WarehouseAddressId { get; set; }

    public OrderSource OrderSource { get; set; } = OrderSource.Online;

    public decimal ItemAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal ShopDiscountAmount { get; set; }

    public decimal PlatformDiscountAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal SellerEarnings { get; set; }

    public SubOrderStatus Status { get; set; } = SubOrderStatus.PendingPayment;

    public string? CancelReason { get; set; }

    public Guid? CancelledBy { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    public DateTimeOffset? ShippedAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}
