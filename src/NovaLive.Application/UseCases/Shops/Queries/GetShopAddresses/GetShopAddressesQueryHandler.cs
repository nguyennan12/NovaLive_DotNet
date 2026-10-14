using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;

public sealed class GetShopAddressesQueryHandler(IShopRepository shops, ShopAccess access) : IQueryHandler<GetShopAddressesQuery, List<ShopAddressResponse>>
{
    public async Task<Result<List<ShopAddressResponse>>> Handle(GetShopAddressesQuery command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(false, ct);
        if (owned.IsFailure) return owned.Error;
        var addresses = await shops.GetSellerAddressesAsync(owned.Value!.Id, ct);
        return addresses.Select(a => a.ToResponse()).ToList();
    }
}
