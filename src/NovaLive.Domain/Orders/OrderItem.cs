using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderItem : Entity
{
    public Guid SubOrderId { get; set; }

    public Guid SkuId { get; set; }

    public string SkuSnapshotJson { get; set; } = "{}";

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }

    public OrderItemStatus Status { get; set; } = OrderItemStatus.Active;
}
