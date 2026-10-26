using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class OrderStatusHistory : Entity
{
    private OrderStatusHistory() { }
    public static OrderStatusHistory Create(Guid subId, SubOrderStatus? from, SubOrderStatus to,
        Guid? actor, OrderRole role, string? note, DateTimeOffset now) => new()
    {
        SubOrderId = subId, FromStatus = from?.ToString(), ToStatus = to.ToString(),
        ChangedBy = actor, ChangedByRole = role, Note = note, ChangedAt = now
    };

    public Guid SubOrderId { get; private set; }

    public string? FromStatus { get; private set; }

    public string ToStatus { get; private set; } = string.Empty;

    public Guid? ChangedBy { get; private set; }

    public OrderRole? ChangedByRole { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; } = DateTimeOffset.UtcNow;
}
