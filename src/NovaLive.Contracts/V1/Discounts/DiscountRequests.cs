namespace NovaLive.Contracts.V1.Discounts;

public record CreateDiscountRequest(
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MaxUses,
    int PerUserLimit,
    string AppliesTo,
    List<Guid>? TargetIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsPublic);

public record UpdateDiscountRequest(
    string Name,
    int? MaxUses,
    DateTimeOffset ValidTo,
    bool IsActive);

public record ValidateVoucherRequest(
    string Code,
    Guid? ShopId,
    List<Guid> CartItemIds);

// Backwards-compatible aliases
public record CreateVoucherRequest(
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MaxUses,
    int PerUserLimit,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsPublic) : CreateDiscountRequest(
        Code,
        Name,
        DiscountType,
        DiscountValue,
        MinOrderAmount,
        MaxDiscountAmount,
        MaxUses,
        PerUserLimit,
        "AllProducts",
        null,
        ValidFrom,
        ValidTo,
        IsPublic);

public record UpdateVoucherRequest(
    string Name,
    int? MaxUses,
    DateTimeOffset ValidTo,
    bool IsActive) : UpdateDiscountRequest(
        Name,
        MaxUses,
        ValidTo,
        IsActive);
