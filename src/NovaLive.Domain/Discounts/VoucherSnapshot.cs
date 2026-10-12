using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed record VoucherSnapshot(
    Guid Id,
    Guid? ShopId,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    DiscountAppliesTo AppliesTo,
    IReadOnlyList<Guid>? TargetIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsActive)
{
    public (bool IsValid, string? ErrorCode, string? ErrorMessage) CheckValidity(DateTimeOffset currentTime)
    {
        if (!IsActive)
            return (false, "Voucher.Inactive", "Voucher chưa được kích hoạt.");

        if (currentTime < ValidFrom)
            return (false, "Voucher.NotYetActive", "Voucher chưa đến thời gian hiệu lực.");

        if (currentTime > ValidTo)
            return (false, "Voucher.Expired", "Voucher đã hết hạn sử dụng.");

        return (true, null, null);
    }

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

    public bool MeetsMinOrderAmount(decimal amount) =>
        amount >= MinOrderAmount;

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

    public bool HasIntegerAmounts()
    {
        if (DiscountType is DiscountType.FixedCart or DiscountType.FreeShip && DiscountValue % 1 != 0)
            return false;

        if (MinOrderAmount % 1 != 0)
            return false;

        if (MaxDiscountAmount.HasValue && MaxDiscountAmount.Value % 1 != 0)
            return false;

        return true;
    }
}
