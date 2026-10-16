using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders;
public sealed class OrderShopAccess(ShopAccess access, ICurrentShop currentShop)
{
    public async Task<Result<Guid>> GetAsync(CancellationToken ct)
    {
        if (currentShop.ShopId is not { } id || id == Guid.Empty) return OrderErrors.ShopContextRequired;
        var owned = await access.GetSellerOwnedAsync(false, ct);
        // CurrentShop may read X-Shop-Id: never treat that client-controlled context as proof of ownership.
        if (owned.IsFailure || owned.Value!.Id != id) return OrderErrors.SubOrderNotFound;
        return id;
    }
}
