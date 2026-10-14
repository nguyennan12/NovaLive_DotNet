using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Carts;
using NovaLive.Domain.Products;
namespace NovaLive.Application.UseCases.Carts;
public static class CartMappings
{
    public static CartItemDto ToResponse(this CartItem item, Sku sku, Spu spu, int availableStock) =>
        new(item.Id, sku.Id, spu.Id, spu.Name, sku.SkuCode, sku.AttributesJson ?? "{}",
            spu.ThumbnailUrl ?? string.Empty, sku.SellPrice, item.Quantity, availableStock,
            sku.SellPrice * item.Quantity);
}
