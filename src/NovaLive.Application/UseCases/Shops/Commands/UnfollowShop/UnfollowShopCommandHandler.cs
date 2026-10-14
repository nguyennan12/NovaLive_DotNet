using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;

public sealed class UnfollowShopCommandHandler(IShopRepository shops, ICurrentUser currentUser) : ICommandHandler<UnfollowShopCommand>
{
    public async Task<Result> Handle(UnfollowShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var shop = await shops.GetActiveByIdAsync(command.ShopId, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (shop.OwnerId == userId) return ShopErrors.SelfFollow;
        // Atomic and idempotent even when concurrent requests delete the same follower.
        await shops.RemoveFollowerAsync(shop.Id, userId, ct);
        return Result.Success();
    }
}
