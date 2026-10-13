using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;

public sealed class GetShopPublicQueryHandler(IAppDbContext db, ICurrentUser currentUser) : IQueryHandler<GetShopPublicQuery, ShopDetailResponse>
{
    public async Task<Result<ShopDetailResponse>> Handle(GetShopPublicQuery command, CancellationToken ct)
    {
        var shop = await db.Shops.AsNoTracking().SingleOrDefaultAsync(s => s.Id == command.ShopId
            && s.Status == ShopStatus.Active && s.DeletedAt == null, ct);
        if (shop is null) return ShopErrors.NotFound;
        var followers = await db.ShopFollowers.CountAsync(f => f.ShopId == shop.Id, ct);
        var products = await db.Spus.CountAsync(p => p.ShopId == shop.Id
            && p.Status == ProductStatus.Active && p.DeletedAt == null, ct);
        var following = currentUser.IsAuthenticated && currentUser.UserId.HasValue
            && await db.ShopFollowers.AnyAsync(f => f.ShopId == shop.Id && f.UserId == currentUser.UserId, ct);
        var addresses = await db.ShopAddresses.AsNoTracking().Where(a => a.ShopId == shop.Id)
            .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.Id).ToListAsync(ct);
        // Explicit public contract projection; never load verification/bank data here.
        return new ShopDetailResponse(shop.Id, shop.OwnerId, shop.ShopName, shop.Slug, shop.Description ?? "",
            shop.LogoUrl, shop.BannerUrl, shop.Phone ?? "", shop.Email ?? "", shop.Status.ToString(),
            (double)shop.RatingAvg, followers, shop.CreatedAt.UtcDateTime,
            addresses.Select(a => a.ToResponse()).ToList(), shop.RatingCount, products, following);
    }
}
