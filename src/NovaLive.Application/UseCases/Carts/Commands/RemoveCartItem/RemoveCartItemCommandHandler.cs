using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Carts.Commands.RemoveCartItem;
public sealed class RemoveCartItemCommandHandler(ICartRepository carts, ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<RemoveCartItemCommand>
{
    public async Task<Result> Handle(RemoveCartItemCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CartErrors.Unauthorized;
        await carts.LockUserAsync(userId, ct);
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null) return CartErrors.ItemNotFound;
        var items = await carts.GetItemsAsync(cart.Id, ct);
        var removed = cart.RemoveSelectedItems(items, [command.CartItemId], clock.UtcNow);
        if (removed.Count == 0) return CartErrors.ItemNotFound;
        carts.RemoveItems(removed);
        return Result.Success();
    }
}
