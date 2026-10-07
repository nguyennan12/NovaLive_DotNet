using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderReturnItem : Entity
{
    public Guid ReturnId { get; private set; }

    public Guid OrderItemId { get; private set; }

    public int Quantity { get; private set; }

    public string? ReasonDetail { get; private set; }
}
