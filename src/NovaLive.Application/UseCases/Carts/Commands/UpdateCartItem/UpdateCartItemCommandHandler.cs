using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
public sealed class UpdateCartItemCommandHandler(ICartRepository carts, ISkuRepository skus,
    IInventoryRepository inventories, ICurrentUser currentUser, IDateTimeProvider clock)
    : ICommandHandler<UpdateCartItemCommand>
{
    public async Task<Result> Handle(UpdateCartItemCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CartErrors.Unauthorized;
        await carts.LockUserAsync(userId, ct);
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null) return CartErrors.ItemNotFound;
        var items = await carts.GetItemsAsync(cart.Id, ct);
        var item = items.SingleOrDefault(i => i.Id == command.CartItemId);
        if (item is null) return CartErrors.ItemNotFound;
        if (command.Request.Quantity == 0)
        {
            carts.RemoveItems(cart.RemoveSelectedItems(items, [item.Id], clock.UtcNow));
            return Result.Success();
        }
        var sku = await skus.GetByIdAsync(item.SkuId, ct);
        if (sku is null || sku.DeletedAt is not null || !sku.IsActive) return CartErrors.SkuUnavailable;
        var stocks = await inventories.GetBySkuIdsAsync([item.SkuId], ct);
        var available = stocks.TryGetValue(item.SkuId, out var stock) && stock.ShopId == item.ShopId
            ? stock.AvailableQty : 0;
        if (command.Request.Quantity > available) return CartErrors.InsufficientStock;
        return cart.UpdateItem(item, command.Request.Quantity, clock.UtcNow);
    }
}
