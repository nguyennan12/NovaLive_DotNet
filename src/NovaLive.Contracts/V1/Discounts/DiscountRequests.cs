namespace NovaLive.Contracts.V1.Discounts;

public record CreateVoucherRequest(
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MaxUses,
    int PerUserLimit,
    DateTime ValidFrom,
    DateTime ValidTo,
    bool IsPublic);

public record UpdateVoucherRequest(
    string Name,
    int? MaxUses,
    DateTime ValidTo,
    bool IsActive);

public record ValidateVoucherRequest(
    string Code,
    Guid? ShopId,
    List<Guid> CartItemIds);
