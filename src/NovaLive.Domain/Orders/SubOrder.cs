using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class SubOrder : AuditableEntity
{
    private SubOrder() { }
    public static SubOrder Create(Guid parentId, string code, Guid warehouseId,
        NovaLive.Domain.Pricing.ShopCalculationResult price, bool cod, DateTimeOffset now,
        OrderSource orderSource = OrderSource.Online) => new()
    {
        ParentOrderId = parentId, SubOrderCode = code, ShopId = price.ShopId, WarehouseAddressId = warehouseId,
        ItemAmount = price.ItemsSubtotal, ShippingFee = price.ShippingFee, ShopDiscountAmount = price.ShopDiscountAmount,
        PlatformDiscountAmount = price.PlatformDiscountAmount, ShippingDiscountAmount = price.ShippingDiscountAmount,
        SubTotal = price.ShopTotal, Status = cod ? SubOrderStatus.Confirmed : SubOrderStatus.PendingPayment,
        ConfirmedAt = cod ? now : null, CreatedAt = now, UpdatedAt = now, OrderSource = orderSource
        // T32: calculate SellerEarnings using the configured commission policy at settlement.
    };

    public Guid ParentOrderId { get; private set; }

    public Guid ShopId { get; private set; }

    public string SubOrderCode { get; private set; } = string.Empty;

    public Guid? WarehouseAddressId { get; private set; }

    public OrderSource OrderSource { get; private set; } = OrderSource.Online;

    public decimal ItemAmount { get; private set; }

    public decimal ShippingFee { get; private set; }

    public decimal ShopDiscountAmount { get; private set; }

    public decimal PlatformDiscountAmount { get; private set; }

    public decimal ShippingDiscountAmount { get; private set; }

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

    public Result MarkAsShipping(DateTimeOffset? shippedAt = null)
    {
        if (Status != SubOrderStatus.Confirmed)
            return Result.Failure(OrderStateErrors.InvalidStatusTransition);

        Status = SubOrderStatus.Shipping;
        ShippedAt = shippedAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = ShippedAt.Value;
        return Result.Success();
    }

    public bool MarkAsDelivered(DateTimeOffset deliveredAt)
    {
        if (Status != SubOrderStatus.Shipping) return false;
        Status = SubOrderStatus.Delivered;
        DeliveredAt = deliveredAt;
        UpdatedAt = deliveredAt;
        return true;
    }
    public Result Cancel(Guid actor, OrderRole role, string reason, DateTimeOffset now)
    {
        if (Status == SubOrderStatus.Cancelled) return Result.Failure(OrderStateErrors.AlreadyCancelled);
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500) return Result.Failure(OrderStateErrors.InvalidReason);
        if (role == OrderRole.Buyer && Status != SubOrderStatus.PendingPayment)
            return Result.Failure(OrderStateErrors.CannotCancelConfirmedOrder);
        if (role is not (OrderRole.Buyer or OrderRole.Seller)
            || Status is not (SubOrderStatus.PendingPayment or SubOrderStatus.Confirmed or SubOrderStatus.Processing))
            return Result.Failure(OrderStateErrors.InvalidStatusTransition);
        Status = SubOrderStatus.Cancelled; CancelReason = reason.Trim(); CancelledBy = actor;
        CancelledAt = now; UpdatedAt = now;
        return Result.Success();
    }
    public Result AssignWarehouse(Guid warehouseId, DateTimeOffset now)
    {
        if (warehouseId == Guid.Empty) return Result.Failure(OrderStateErrors.InvalidWarehouse);
        if (Status == SubOrderStatus.PendingPayment) return Result.Failure(OrderStateErrors.SubOrderNotPaid);
        if (Status != SubOrderStatus.Confirmed) return Result.Failure(OrderStateErrors.InvalidStatusTransition);
        WarehouseAddressId = warehouseId; UpdatedAt = now;
        // Keep Confirmed: T35 requires this status to create a shipment.
        return Result.Success();
    }

    public void ConfirmPayment(DateTimeOffset now)
    {
        if (Status == SubOrderStatus.PendingPayment)
        {
            Status = SubOrderStatus.Confirmed;
            ConfirmedAt = now;
            UpdatedAt = now;
        }
    }
}

