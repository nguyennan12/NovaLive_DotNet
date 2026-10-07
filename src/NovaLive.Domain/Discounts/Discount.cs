using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class Discount : AuditableEntity
{
    public Guid? ShopId { get; set; }

    public Guid CreatedBy { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? MaxUses { get; set; }

    public int UsedCount { get; set; }

    public int PerUserLimit { get; set; } = 1;

    public DiscountAppliesTo AppliesTo { get; set; } = DiscountAppliesTo.AllProducts;

    public List<Guid>? TargetIds { get; set; }

    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset ValidTo { get; set; }

    public bool IsPublic { get; set; } = true;

    public bool IsActive { get; set; } = true;
}
