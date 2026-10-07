using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderItem : Entity
{
    public Guid SubOrderId { get; private set; }

    public Guid SkuId { get; private set; }

    public string SkuSnapshotJson { get; private set; } = "{}";

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal OriginalPrice { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal LineTotal { get; private set; }

    public OrderItemStatus Status { get; private set; } = OrderItemStatus.Active;
}
