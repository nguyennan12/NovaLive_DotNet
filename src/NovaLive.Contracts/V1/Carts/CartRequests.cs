namespace NovaLive.Contracts.V1.Carts;

public record AddToCartRequest(
    Guid SkuId,
    int Quantity,
    Guid? LivestreamSessionId = null);

public record UpdateCartItemRequest(
    int Quantity);

public record RemoveCartItemsRequest(
    List<Guid> CartItemIds);
