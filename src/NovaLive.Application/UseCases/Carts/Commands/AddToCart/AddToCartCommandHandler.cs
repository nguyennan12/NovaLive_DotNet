using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Contracts.V1.Carts;
using NovaLive.Domain.Carts;
namespace NovaLive.Application.UseCases.Carts.Commands.AddToCart;
public sealed class AddToCartCommandHandler(ICartRepository carts, ISkuRepository skus,
    ISpuRepository spus, IShopRepository shops, IInventoryRepository inventories,
    ICurrentUser currentUser, IDateTimeProvider clock) : ICommandHandler<AddToCartCommand, CartItemDto>
{
    public async Task<Result<CartItemDto>> Handle(AddToCartCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CartErrors.Unauthorized;
        await carts.LockUserAsync(userId, ct);
        var sku = await skus.GetByIdAsync(command.Request.SkuId, ct);
        if (sku is null) return CartErrors.SkuNotFound;
        if (!sku.IsActive || sku.DeletedAt is not null) return CartErrors.SkuUnavailable;
        var spu = await spus.GetByIdAsync(sku.SpuId, ct);
        if (spu is null || spu.DeletedAt is not null || spu.Status != ProductStatus.Active || spu.ShopId != sku.ShopId)
            return CartErrors.SpuUnavailable;
        var shop = await shops.GetActiveByIdAsync(sku.ShopId, ct);
        if (shop is null || shop.DeletedAt is not null || shop.Status != ShopStatus.Active)
            return CartErrors.ShopUnavailable;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            // Refresh stock and cart after a losing unique insert; recheck the accumulated quantity.
            var stocks = await inventories.GetBySkuIdsAsync([sku.Id], ct);
            var available = stocks.TryGetValue(sku.Id, out var stock) && stock.ShopId == sku.ShopId
                ? stock.AvailableQty : 0;
            if (available <= 0) return CartErrors.OutOfStock;
            var cart = await carts.GetByUserIdAsync(userId, ct);
            var items = cart is null ? [] : await carts.GetItemsAsync(cart.Id, ct);
            var existing = items.SingleOrDefault(i => i.SkuId == sku.Id);
            if ((long)(existing?.Quantity ?? 0) + command.Request.Quantity > available)
                return CartErrors.InsufficientStock;
            var now = clock.UtcNow;
            if (cart is null)
            {
                cart = new Cart(userId, now);
                await carts.AddAsync(cart, ct);
            }
            var result = cart.AddItem(items, sku.Id, sku.ShopId, command.Request.Quantity, sku.SellPrice, now);
            if (result.IsFailure) return result.Error;
            var item = result.Value!;
            if (existing is null) await carts.AddItemAsync(item, ct);
            if (await carts.TrySaveAsync(userId, ct))
                return item.ToResponse(sku, spu, available);
        }
        return CartErrors.ConcurrentChange;
    }
}
