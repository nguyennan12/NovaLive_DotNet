using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class DiscountUsage : Entity
{
    public Guid DiscountId { get; set; }

    public Guid UserId { get; set; }

    public Guid ParentOrderId { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;
}
