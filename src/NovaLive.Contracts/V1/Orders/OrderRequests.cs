namespace NovaLive.Contracts.V1.Orders;

public record CalculateCheckoutRequest(
    List<Guid> CartItemIds,
    Guid ShippingAddressId,
    List<ShopVoucherSelectionDto>? ShopVouchers,
    string? PlatformProductVoucherCode,
    string? PlatformFreeshipVoucherCode);

public record SubmitCheckoutRequest(
    List<Guid> CartItemIds,
    Guid ShippingAddressId,
    List<ShopShippingSelectionDto> ShippingProviders,
    List<ShopVoucherSelectionDto>? ShopVouchers,
    string? PlatformProductVoucherCode,
    string? PlatformFreeshipVoucherCode,
    string PaymentMethod,
    string? Note);

public record ShopVoucherSelectionDto(
    Guid ShopId,
    string Code);

public record ShopShippingSelectionDto(
    Guid ShopId,
    string Provider,
    string ServiceCode);

public record CancelOrderRequest(
    string Reason);

public record ConfirmSubOrderRequest(
    Guid WarehouseAddressId);
