using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() { }
    public static OrderItem Create(Guid subOrderId, string snapshot, decimal originalPrice,
        NovaLive.Domain.Pricing.OrderItemCalculationResult price) => new()
    {
        SubOrderId = subOrderId, SkuId = price.SkuId, SkuSnapshotJson = snapshot,
        Quantity = price.Quantity, UnitPrice = price.UnitPrice, OriginalPrice = originalPrice,
        DiscountAmount = price.TotalDiscountAmount, LineTotal = price.FinalLineTotal
    };

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
