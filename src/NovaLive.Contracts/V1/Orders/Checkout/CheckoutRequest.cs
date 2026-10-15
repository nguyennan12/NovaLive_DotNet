namespace NovaLive.Contracts.V1.Orders;
public sealed record CheckoutRequest(
    IReadOnlyList<Guid> CartItemIds, Guid ShippingAddressId,
    IReadOnlyList<ShopShippingSelectionDto> ShippingProviders,
    IReadOnlyList<ShopVoucherSelectionDto>? ShopVouchers,
    string? PlatformProductVoucherCode, string? PlatformFreeshipVoucherCode,
    string PaymentMethod, string? Note);
