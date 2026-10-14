using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.FollowShop;

public sealed class FollowShopCommandHandler(IShopRepository shops, ICurrentUser currentUser) : ICommandHandler<FollowShopCommand>
{
    public async Task<Result> Handle(FollowShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var shop = await shops.GetActiveByIdAsync(command.ShopId, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (shop.OwnerId == userId) return ShopErrors.SelfFollow;
        if (await shops.IsFollowingAsync(shop.Id, userId, ct))
            return Result.Success();
        await shops.AddFollowerAsync(new ShopFollower(shop.Id, userId), ct);
        await shops.SaveFollowAsync(ct);
        return Result.Success();
    }
}
