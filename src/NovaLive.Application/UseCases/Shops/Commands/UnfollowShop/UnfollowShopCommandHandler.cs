using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;

public sealed class UnfollowShopCommandHandler(IAppDbContext db, ICurrentUser currentUser) : ICommandHandler<UnfollowShopCommand>
{
    public async Task<Result> Handle(UnfollowShopCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        var shop = await db.Shops.AsNoTracking().SingleOrDefaultAsync(s => s.Id == command.ShopId
            && s.Status == ShopStatus.Active && s.DeletedAt == null, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (shop.OwnerId == userId) return ShopErrors.SelfFollow;
        // Atomic and idempotent even when concurrent requests delete the same follower.
        await db.ShopFollowers.Where(f => f.ShopId == shop.Id && f.UserId == userId).ExecuteDeleteAsync(ct);
        return Result.Success();
    }
}
