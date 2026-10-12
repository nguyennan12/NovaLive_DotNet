using NovaLive.Domain.Common;

namespace NovaLive.Domain.Discounts;

public sealed class Discount : AuditableEntity
{
    private Discount() { }

    public Discount(
        Guid? shopId,
        string code,
        string name,
        DiscountType discountType,
        decimal discountValue,
        decimal minOrderAmount,
        decimal? maxDiscountAmount,
        int? maxUses,
        int perUserLimit,
        DiscountAppliesTo appliesTo,
        List<Guid>? targetIds,
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        bool isPublic,
        bool isActive = true)
    {
        ShopId = shopId;
        Code = code;
        Name = name;
        DiscountType = discountType;
        DiscountValue = discountValue;
        MinOrderAmount = minOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;
        MaxUses = maxUses;
        UsedCount = 0;
        PerUserLimit = perUserLimit;
        AppliesTo = appliesTo;
        TargetIds = targetIds;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsPublic = isPublic;
        IsActive = isActive;
    }

    public static Result<Discount> CreateShopDiscount(
        Guid shopId,
        string code,
        string name,
        DiscountType discountType,
        decimal discountValue,
        decimal minOrderAmount,
        decimal? maxDiscountAmount,
        int? maxUses,
        int perUserLimit,
        DiscountAppliesTo appliesTo,
        List<Guid>? targetIds,
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        bool isPublic)
    {
        if (discountType is not (DiscountType.PercentCart or DiscountType.FixedCart))
        {
            return Result.Failure<Discount>(Error.Validation(
                "Discounts.InvalidType", "Voucher Shop chỉ hỗ trợ loại PercentCart hoặc FixedCart."));
        }

        var validationResult = ValidateCommon(validFrom, validTo, minOrderAmount, discountType, discountValue, maxDiscountAmount, perUserLimit, maxUses);
        if (validationResult.IsFailure)
            return Result.Failure<Discount>(validationResult.Error);

        return Result.Success(new Discount(
            shopId,
            code.Trim().ToUpperInvariant(),
            name.Trim(),
            discountType,
            discountValue,
            minOrderAmount,
            maxDiscountAmount,
            maxUses,
            perUserLimit,
            appliesTo,
            targetIds,
            validFrom,
            validTo,
            isPublic));
    }

    public static Result<Discount> CreatePlatformDiscount(
        string code,
        string name,
        DiscountType discountType,
        decimal discountValue,
        decimal minOrderAmount,
        decimal? maxDiscountAmount,
        int? maxUses,
        int perUserLimit,
        DiscountAppliesTo appliesTo,
        List<Guid>? targetIds,
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        bool isPublic)
    {
        var validationResult = ValidateCommon(validFrom, validTo, minOrderAmount, discountType, discountValue, maxDiscountAmount, perUserLimit, maxUses);
        if (validationResult.IsFailure)
            return Result.Failure<Discount>(validationResult.Error);

        return Result.Success(new Discount(
            null,
            code.Trim().ToUpperInvariant(),
            name.Trim(),
            discountType,
            discountValue,
            minOrderAmount,
            maxDiscountAmount,
            maxUses,
            perUserLimit,
            appliesTo,
            targetIds,
            validFrom,
            validTo,
            isPublic));
    }

    private static Result ValidateCommon(
        DateTimeOffset validFrom,
        DateTimeOffset validTo,
        decimal minOrderAmount,
        DiscountType discountType,
        decimal discountValue,
        decimal? maxDiscountAmount,
        int perUserLimit,
        int? maxUses)
    {
        if (validTo <= validFrom)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidDateRange", "Thời gian kết thúc (valid_to) phải lớn hơn thời gian bắt đầu (valid_from)."));
        }

        if (minOrderAmount < 0)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidMinOrderAmount", "Giá trị đơn hàng tối thiểu không được âm."));
        }

        if (discountValue <= 0)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidDiscountValue", "Giá trị giảm giá phải lớn hơn 0."));
        }

        if (discountType is DiscountType.PercentCart or DiscountType.PercentShip)
        {
            if (discountValue > 100)
            {
                return Result.Failure(Error.Validation(
                    "Discounts.InvalidPercentValue", "Phần trăm giảm giá không được vượt quá 100%."));
            }

            if (!maxDiscountAmount.HasValue || maxDiscountAmount.Value <= 0)
            {
                return Result.Failure(Error.Validation(
                    "Discounts.MaxDiscountAmountRequired", "Giảm giá phần trăm bắt buộc phải có mức giảm tối đa lớn hơn 0."));
            }
        }

        if (perUserLimit <= 0)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidPerUserLimit", "Giới hạn mỗi người dùng phải lớn hơn 0."));
        }

        if (maxUses.HasValue && maxUses.Value <= 0)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidMaxUses", "Tổng số lượt dùng phải lớn hơn 0."));
        }

        return Result.Success();
    }

    public Result Update(string name, int? maxUses, DateTimeOffset validTo, bool isActive)
    {
        if (validTo <= ValidFrom)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidDateRange", "Thời gian kết thúc (valid_to) phải lớn hơn thời gian bắt đầu (valid_from)."));
        }

        if (maxUses.HasValue && maxUses.Value < UsedCount)
        {
            return Result.Failure(Error.Validation(
                "Discounts.InvalidMaxUses", $"Tổng lượt dùng mới ({maxUses.Value}) không được nhỏ hơn số lượt đã dùng ({UsedCount})."));
        }

        Name = name.Trim();
        MaxUses = maxUses;
        ValidTo = validTo;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void IncrementUsedCount()
    {
        UsedCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

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
