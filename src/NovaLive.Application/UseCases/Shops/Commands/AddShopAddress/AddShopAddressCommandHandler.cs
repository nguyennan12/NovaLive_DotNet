using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;

public sealed class AddShopAddressCommandHandler(IShopRepository shops, ShopAccess access, ShopAddressDefaults defaults) : ICommandHandler<AddShopAddressCommand, ShopAddressResponse>
{
    public async Task<Result<ShopAddressResponse>> Handle(AddShopAddressCommand command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(true, ct);
        if (owned.IsFailure) return owned.Error;
        var addresses = await shops.GetAddressesAsync(owned.Value!.Id, ct);
        var req = command.Request;
        var address = new ShopAddress(owned.Value!.Id, req.WarehouseName, req.ContactName, req.ContactPhone,
            req.ProvinceId!.Value, req.ProvinceName, req.DistrictId!.Value, req.DistrictName,
            req.WardCode!, req.WardName, req.DetailAddress);
        await defaults.SelectAsync(addresses, address, req.IsPrimary, req.IsReturn, ct);
        await shops.AddAddressAsync(address, ct);
        return address.ToResponse();
    }
}
