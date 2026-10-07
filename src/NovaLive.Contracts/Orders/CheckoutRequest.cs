namespace NovaLive.Contracts.Orders;

public sealed record CheckoutRequest(
    IReadOnlyList<Guid> CartItemIds,
    Guid ShippingAddressId,
    string PaymentMethod,
    string? PlatformProductVoucherCode,
    string? PlatformFreeshipVoucherCode,
    string? Note);
