namespace NovaLive.Contracts.V1.Discounts;

public record DiscountResponse(
    Guid Id,
    Guid? ShopId,
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MaxUses,
    int UsedCount,
    int PerUserLimit,
    string AppliesTo,
    List<Guid>? TargetIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsPublic,
    bool IsActive,
    DateTimeOffset CreatedAt);

public record VoucherValidationResponse(
    bool IsValid,
    string Code,
    decimal DiscountAmount,
    string? ReasonIfInvalid);

// Backwards-compatible aliases
public record VoucherResponse(
    Guid Id,
    Guid? ShopId,
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MaxUses,
    int UsedCount,
    int PerUserLimit,
    DateTime ValidFrom,
    DateTime ValidTo,
    bool IsPublic,
    bool IsActive);

public record VoucherValidationResultDto(
    bool IsValid,
    string Code,
    decimal DiscountAmount,
    string? ErrorMessage);
