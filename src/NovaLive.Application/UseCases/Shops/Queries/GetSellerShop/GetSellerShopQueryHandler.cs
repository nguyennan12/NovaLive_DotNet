using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetSellerShop;

public sealed class GetSellerShopQueryHandler(IAppDbContext db, ShopAccess access) : IQueryHandler<GetSellerShopQuery, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(GetSellerShopQuery command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;
        var verification = await db.ShopVerifications.AsNoTracking().Where(v => v.ShopId == shop.Id)
            .OrderByDescending(v => v.SubmittedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct);
        var followers = await db.ShopFollowers.CountAsync(f => f.ShopId == shop.Id, ct);
        return shop.ToResponse(followers, verification);
    }
}
