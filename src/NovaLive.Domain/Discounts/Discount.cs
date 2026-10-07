using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class Discount : AuditableEntity
{
    public Guid? ShopId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public DiscountType DiscountType { get; private set; }

    public decimal DiscountValue { get; private set; }

    public decimal MinOrderAmount { get; private set; }

    public decimal? MaxDiscountAmount { get; private set; }

    public int? MaxUses { get; private set; }

    public int UsedCount { get; private set; }

    public int PerUserLimit { get; private set; } = 1;

    public DiscountAppliesTo AppliesTo { get; private set; } = DiscountAppliesTo.AllProducts;

    public List<Guid>? TargetIds { get; private set; }

    public DateTimeOffset ValidFrom { get; private set; }

    public DateTimeOffset ValidTo { get; private set; }

    public bool IsPublic { get; private set; } = true;

    public bool IsActive { get; private set; } = true;
}
