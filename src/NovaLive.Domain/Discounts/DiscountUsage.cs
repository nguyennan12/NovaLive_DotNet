using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class DiscountUsage : Entity
{
    private DiscountUsage() { }
    public static DiscountUsage Create(Guid discountId, Guid userId, Guid parentId, decimal amount, DateTimeOffset now) => new()
    { DiscountId = discountId, UserId = userId, ParentOrderId = parentId, DiscountAmount = amount, UsedAt = now };

    public Guid DiscountId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ParentOrderId { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public DateTimeOffset UsedAt { get; private set; } = DateTimeOffset.UtcNow;
}
