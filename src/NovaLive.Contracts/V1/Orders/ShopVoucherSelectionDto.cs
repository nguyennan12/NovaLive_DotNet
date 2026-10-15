namespace NovaLive.Contracts.V1.Orders;

public record ShopVoucherSelectionDto(
    Guid ShopId,
    string Code);
