namespace NovaLive.Contracts.V1.Carts;

public record AddToCartRequest(
    Guid SkuId,
    int Quantity);

public record UpdateCartItemRequest(
    int Quantity);

public record RemoveCartItemsRequest(
    List<Guid> CartItemIds);
