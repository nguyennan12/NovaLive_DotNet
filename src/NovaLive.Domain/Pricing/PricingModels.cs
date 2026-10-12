using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;

namespace NovaLive.Domain.Pricing;

public sealed record OrderItemCalculationInput(
    Guid SkuId,
    Guid SpuId,
    Guid? CategoryId,
    int Quantity,
    decimal UnitPrice);

public sealed record ShopCalculationInput(
    Guid ShopId,
    decimal ShippingFee,
    IReadOnlyList<OrderItemCalculationInput> Items,
    VoucherSnapshot? ShopVoucher);

public sealed record PriceCalculationInput(
    IReadOnlyList<ShopCalculationInput> Shops,
    VoucherSnapshot? PlatformProductVoucher,
    VoucherSnapshot? PlatformShippingVoucher,
    DateTimeOffset CurrentTime);

public sealed record OrderItemCalculationResult(
    Guid SkuId,
    Guid SpuId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal ShopDiscountAmount,
    decimal PlatformDiscountAmount,
    decimal TotalDiscountAmount,
    decimal FinalLineTotal);

public sealed record ShopCalculationResult(
    Guid ShopId,
    decimal ItemsSubtotal,
    decimal ShippingFee,
    decimal ShopDiscountAmount,
    decimal PlatformDiscountAmount,
    decimal ShippingDiscountAmount,
    decimal NetGoodsAmount,
    decimal ShopTotal,
    IReadOnlyList<OrderItemCalculationResult> Items);

public sealed record VoucherEvaluationResult(
    string VoucherCode,
    DiscountType DiscountType,
    Guid? ShopId,
    bool IsApplied,
    decimal DiscountAmount,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record PriceCalculationResult(
    decimal TotalItemAmount,
    decimal TotalShippingFee,
    decimal TotalShopDiscount,
    decimal TotalPlatformDiscount,
    decimal TotalShippingDiscount,
    decimal GrandTotal,
    IReadOnlyList<ShopCalculationResult> ShopResults,
    IReadOnlyList<VoucherEvaluationResult> VoucherEvaluations);
