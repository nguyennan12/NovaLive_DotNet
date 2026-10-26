using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns;
public sealed class SellerReturnAccess(IOrderReturnRepository returns, ISubOrderRepository subs, OrderShopAccess shops, ICurrentUser user)
{
    public async Task<Result<SellerReturnContext>> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } seller) return ReturnErrors.Unauthorized;
        var shop = await shops.GetAsync(ct);
        if (shop.IsFailure) return ReturnErrors.NotFound;
        var request = await returns.FindAsync(id, ct);
        if (request is null) return ReturnErrors.NotFound;
        var owned = await subs.FindByIdAsync(request.SubOrderId, ct);
        if (owned?.ShopId != shop.Value) return ReturnErrors.NotFound;
        // All seller mutations lock sub-order then return, consistent with request creation.
        var sub = await subs.GetForUpdateAsync(owned.Id, ct);
        if (sub?.ShopId != shop.Value) return ReturnErrors.NotFound;
        var locked = await returns.GetForUpdateAsync(id, ct);
        if (locked is null || locked.SubOrderId != sub.Id) return ReturnErrors.NotFound;
        return new SellerReturnContext(locked, sub, seller);
    }
}
