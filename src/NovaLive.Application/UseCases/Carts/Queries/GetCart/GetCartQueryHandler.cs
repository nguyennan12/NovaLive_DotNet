using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Contracts.V1.Carts;
namespace NovaLive.Application.UseCases.Carts.Queries.GetCart;
public sealed class GetCartQueryHandler(ICartRepository carts, ISkuRepository skus, ISpuRepository spus,
    IShopRepository shops, IInventoryRepository inventories, ICurrentUser currentUser)
    : IQueryHandler<GetCartQuery, CartResponse>
{
    public async Task<Result<CartResponse>> Handle(GetCartQuery query, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CartErrors.Unauthorized;
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null) return new CartResponse(userId, 0, 0m, []);
        var items = await carts.GetItemsAsync(cart.Id, ct);
        if (items.Count == 0) return new CartResponse(userId, 0, 0m, []);
        var skuIds = items.Select(i => i.SkuId).Distinct().ToList();
        // Sequential database calls: repositories share the scoped EF context.
        var skuMap = (await skus.GetByIdsAsync(skuIds, ct)).Where(s => s.DeletedAt is null).ToDictionary(s => s.Id);
        var spuMap = (await spus.GetByIdsAsync(skuMap.Values.Select(s => s.SpuId).Distinct(), ct))
            .Where(s => s.DeletedAt is null).ToDictionary(s => s.Id);
        var shopMap = (await shops.GetByIdsAsync(items.Select(i => i.ShopId).Distinct(), ct))
            .Where(s => s.DeletedAt is null).ToDictionary(s => s.Id);
        var stocks = await inventories.GetBySkuIdsAsync(skuIds, ct);
        var groups = new List<ShopCartGroupDto>();
        foreach (var group in items.Where(i => i.CartId == cart.Id).GroupBy(i => i.ShopId))
        {
            if (!shopMap.TryGetValue(group.Key, out var shop)) continue;
            var responses = new List<CartItemDto>();
            foreach (var item in group)
            {
                if (!skuMap.TryGetValue(item.SkuId, out var sku) || sku.ShopId != shop.Id
                    || !spuMap.TryGetValue(sku.SpuId, out var spu) || spu.ShopId != shop.Id) continue;
                var available = stocks.TryGetValue(sku.Id, out var stock) && stock.ShopId == shop.Id
                    ? stock.AvailableQty : 0;
                responses.Add(item.ToResponse(sku, spu, available));
            }
            if (responses.Count > 0) groups.Add(new ShopCartGroupDto(shop.Id, shop.ShopName, responses));
        }
        var displayed = groups.SelectMany(g => g.Items).ToList();
        return new CartResponse(userId, displayed.Count, displayed.Sum(i => i.LineTotal), groups);
    }
}
