using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class SubOrder : AuditableEntity
{
    public Guid ParentOrderId { get; private set; }

    public Guid ShopId { get; private set; }

    public string SubOrderCode { get; private set; } = string.Empty;

    public Guid? WarehouseAddressId { get; private set; }

    public OrderSource OrderSource { get; private set; } = OrderSource.Online;

    public decimal ItemAmount { get; private set; }

    public decimal ShippingFee { get; private set; }

    public decimal ShopDiscountAmount { get; private set; }

    public decimal PlatformDiscountAmount { get; private set; }

    public decimal SubTotal { get; private set; }

    public decimal SellerEarnings { get; private set; }

    public SubOrderStatus Status { get; private set; } = SubOrderStatus.PendingPayment;

    public string? CancelReason { get; private set; }

    public Guid? CancelledBy { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public DateTimeOffset? ShippedAt { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }
}
