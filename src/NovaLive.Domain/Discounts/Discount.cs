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

    public bool IsTimeValid(DateTimeOffset currentTime) =>
        IsActive && currentTime >= ValidFrom && currentTime <= ValidTo;

    public bool MeetsMinOrderAmount(decimal amount) =>
        amount >= MinOrderAmount;

    public bool IsItemEligible(Guid spuId, Guid? categoryId)
    {
        return AppliesTo switch
        {
            DiscountAppliesTo.AllProducts => true,
            DiscountAppliesTo.SpecificSpus => TargetIds != null && TargetIds.Contains(spuId),
            DiscountAppliesTo.SpecificCategories => categoryId.HasValue && TargetIds != null && TargetIds.Contains(categoryId.Value),
            _ => true
        };
    }

    public decimal CalculateDiscount(decimal baseAmount)
    {
        if (baseAmount <= 0)
            return 0m;

        decimal amount = DiscountType switch
        {
            DiscountType.FixedCart or DiscountType.FreeShip =>
                Math.Min(DiscountValue, baseAmount),

            DiscountType.PercentCart or DiscountType.PercentShip =>
                Math.Min(Math.Floor(baseAmount * (DiscountValue / 100m)), MaxDiscountAmount ?? baseAmount),

            _ => 0m
        };

        return Math.Min(amount, baseAmount);
    }

    public VoucherSnapshot ToSnapshot() => new(
        Id,
        ShopId,
        Code,
        DiscountType,
        DiscountValue,
        MinOrderAmount,
        MaxDiscountAmount,
        AppliesTo,
        TargetIds,
        ValidFrom,
        ValidTo,
        IsActive);
}
