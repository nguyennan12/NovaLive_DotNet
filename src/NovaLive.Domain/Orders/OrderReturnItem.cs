using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderReturnItem : Entity
{
    public Guid ReturnId { get; set; }

    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }

    public string? ReasonDetail { get; set; }
}
