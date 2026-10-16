namespace NovaLive.Application.Abstractions.ThirdParty;

public sealed record ShippingFeeRequest(Guid ShopId, Guid WarehouseAddressId, Guid ShippingAddressId, int WeightGram);
