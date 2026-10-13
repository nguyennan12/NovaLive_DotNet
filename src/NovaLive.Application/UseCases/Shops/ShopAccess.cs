using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops;

public sealed class ShopAccess(IAppDbContext db, ICurrentUser currentUser, IShopConcurrencyService concurrency)
{
    public async Task<Result<Shop>> GetSellerOwnedAsync(bool forMutation, CancellationToken ct)
    {
        var result = await GetOwnedAsync(forMutation, ct);
        if (result.IsFailure) return result.Error;
        // Pending owners use GET /shops/me until T14 grants Seller after approval.
        if (result.Value!.Status == ShopStatus.Pending) return ShopErrors.CannotManage;
        return result;
    }

    public async Task<Result<Shop>> GetOwnedAsync(bool forMutation, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId || userId == Guid.Empty)
            return ShopErrors.Unauthorized;
        if (forMutation) await concurrency.LockOwnerAsync(userId, ct);
        // Ownership comes from the database, including Pending shops whose tokens have no shop_id.
        var shop = await db.Shops.SingleOrDefaultAsync(s => s.OwnerId == userId && s.DeletedAt == null, ct);
        if (shop is null) return ShopErrors.NotFound;
        if (forMutation && !shop.CanManage) return ShopErrors.CannotManage;
        return shop;
    }
}
