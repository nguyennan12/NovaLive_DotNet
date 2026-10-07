using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class DiscountUsage : Entity
{
    public Guid DiscountId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ParentOrderId { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public DateTimeOffset UsedAt { get; private set; } = DateTimeOffset.UtcNow;
}
