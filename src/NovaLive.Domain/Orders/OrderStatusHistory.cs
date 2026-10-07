using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderStatusHistory : Entity
{
    public Guid SubOrderId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public Guid? ChangedBy { get; set; }

    public OrderRole? ChangedByRole { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
