namespace NovaLive.Contracts.V1.Carts;

public record CartResponse(
    Guid UserId,
    int TotalItemsCount,
    decimal GrandTotal,
    List<ShopCartGroupDto> ShopGroups);

public record ShopCartGroupDto(
    Guid ShopId,
    string ShopName,
    List<CartItemDto> Items);

public record CartItemDto(
    Guid CartItemId,
    Guid SkuId,
    Guid SpuId,
    string SpuName,
    string SkuCode,
    string AttributesJson,
    string ThumbnailUrl,
    decimal Price,
    int Quantity,
    int AvailableStock,
    decimal LineTotal);
