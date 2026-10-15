namespace NovaLive.Contracts.V1.Orders;

public record SubmitCheckoutRequest(
    List<Guid> CartItemIds,
    Guid ShippingAddressId,
    List<ShopShippingSelectionDto> ShippingProviders,
    List<ShopVoucherSelectionDto>? ShopVouchers,
    string? PlatformProductVoucherCode,
    string? PlatformFreeshipVoucherCode,
    string PaymentMethod,
    string? Note);
