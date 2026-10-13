using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;

public sealed class UnfollowShopCommandHandler(IAppDbContext db, ICurrentUser currentUser, IShopPersistence persistence) : ICommandHandler<UnfollowShopCommand>
{
    public async Task<Result> Handle(UnfollowShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var shop = await db.Shops.AsNoTracking().SingleOrDefaultAsync(s => s.Id == command.ShopId
            && s.Status == ShopStatus.Active && s.DeletedAt == null, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (shop.OwnerId == userId) return ShopErrors.SelfFollow;
        await persistence.RemoveFollowAsync(shop.Id, userId, ct);
        return Result.Success();
    }
}
