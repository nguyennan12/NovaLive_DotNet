using NovaLive.Application.Abstractions.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.FollowShop;

public sealed class FollowShopCommandHandler(IAppDbContext db, ICurrentUser currentUser, IShopRepository concurrency) : ICommandHandler<FollowShopCommand>
{
    public async Task<Result> Handle(FollowShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var shop = await db.Shops.AsNoTracking().SingleOrDefaultAsync(s => s.Id == command.ShopId
            && s.Status == ShopStatus.Active && s.DeletedAt == null, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (shop.OwnerId == userId) return ShopErrors.SelfFollow;
        if (await db.ShopFollowers.AnyAsync(f => f.ShopId == shop.Id && f.UserId == userId, ct))
            return Result.Success();
        db.ShopFollowers.Add(new ShopFollower(shop.Id, userId));
        await concurrency.SaveFollowAsync(ct);
        return Result.Success();
    }
}
