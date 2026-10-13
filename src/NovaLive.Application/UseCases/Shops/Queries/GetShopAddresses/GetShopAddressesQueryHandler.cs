using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;

public sealed class GetShopAddressesQueryHandler(IAppDbContext db, ShopAccess access) : IQueryHandler<GetShopAddressesQuery, List<ShopAddressResponse>>
{
    public async Task<Result<List<ShopAddressResponse>>> Handle(GetShopAddressesQuery command, CancellationToken ct)
    {
        var owned = await access.GetOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var addresses = await db.ShopAddresses.AsNoTracking().Where(a => a.ShopId == owned.Value!.Id)
            .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.CreatedAt).ThenBy(a => a.Id).ToListAsync(ct);
        return addresses.Select(a => a.ToResponse()).ToList();
    }
}
