using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;

public sealed class GetAdminPayoutsQueryHandler(
    ISellerPayoutRepository payouts,
    IShopRepository shops,
    IUserRepository users) : IQueryHandler<GetAdminPayoutsQuery, PagedResult<SellerPayoutResponse>>
{
    public async Task<Result<PagedResult<SellerPayoutResponse>>> Handle(
        GetAdminPayoutsQuery query,
        CancellationToken ct)
    {
        var req = query.Request;
        PayoutStatus? filterStatus = null;
        if (!string.IsNullOrWhiteSpace(req.Status) && Enum.TryParse<PayoutStatus>(req.Status, true, out var parsedStatus))
        {
            filterStatus = parsedStatus;
        }

        var (items, totalCount) = await payouts.GetPagedAsync(
            req.ShopId,
            filterStatus,
            req.NormalizedPage,
            req.NormalizedSize,
            ct);

        var responses = new List<SellerPayoutResponse>();
        foreach (var payout in items)
        {
            var shop = await shops.GetTrackedByIdAsync(payout.ShopId, ct);
            var owner = shop is not null ? await users.GetByIdAsync(shop.OwnerId, ct) : null;
            responses.Add(payout.ToResponse(owner?.FullName ?? shop?.ShopName));
        }

        return PagedResult<SellerPayoutResponse>.Create(
            responses,
            req.NormalizedPage,
            req.NormalizedSize,
            totalCount);
    }
}
