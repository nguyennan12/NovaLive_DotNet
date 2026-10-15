namespace NovaLive.Contracts.V1.Orders;

public record CalculateCheckoutRequest(
    List<Guid> CartItemIds,
    Guid ShippingAddressId,
    List<ShopVoucherSelectionDto>? ShopVouchers,
    string? PlatformProductVoucherCode,
    string? PlatformFreeshipVoucherCode);
