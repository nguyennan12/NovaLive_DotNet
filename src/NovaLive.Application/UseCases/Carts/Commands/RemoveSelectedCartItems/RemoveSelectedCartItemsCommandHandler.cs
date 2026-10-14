using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
public sealed class RemoveSelectedCartItemsCommandHandler(ICartRepository carts, ICurrentUser currentUser,
    IDateTimeProvider clock) : ICommandHandler<RemoveSelectedCartItemsCommand, int>
{
    public async Task<Result<int>> Handle(RemoveSelectedCartItemsCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return CartErrors.Unauthorized;
        await carts.LockUserAsync(userId, ct);
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null) return 0;
        var items = await carts.GetItemsAsync(cart.Id, ct);
        var removed = cart.RemoveSelectedItems(items, command.Request.CartItemIds, clock.UtcNow);
        if (removed.Count > 0) carts.RemoveItems(removed);
        return removed.Count;
    }
}
