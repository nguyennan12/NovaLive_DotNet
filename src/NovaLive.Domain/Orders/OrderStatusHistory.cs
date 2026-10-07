using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderStatusHistory : Entity
{
    public Guid SubOrderId { get; private set; }

    public string? FromStatus { get; private set; }

    public string ToStatus { get; private set; } = string.Empty;

    public Guid? ChangedBy { get; private set; }

    public OrderRole? ChangedByRole { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; } = DateTimeOffset.UtcNow;
}
