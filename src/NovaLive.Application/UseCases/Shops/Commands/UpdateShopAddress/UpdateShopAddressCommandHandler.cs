using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;

public sealed class UpdateShopAddressCommandHandler(IAppDbContext db, ShopAccess access, ShopAddressDefaults defaults) : ICommandHandler<UpdateShopAddressCommand, ShopAddressResponse>
{
    public async Task<Result<ShopAddressResponse>> Handle(UpdateShopAddressCommand command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(true, ct);
        if (owned.IsFailure) return owned.Error;
        var addresses = await db.ShopAddresses.Where(a => a.ShopId == owned.Value!.Id).ToListAsync(ct);
        var address = addresses.SingleOrDefault(a => a.Id == command.Id);
        if (address is null) return ShopErrors.AddressNotFound;
        var req = command.Request;
        // A current default remains selected until another warehouse replaces it.
        await defaults.SelectAsync(addresses, address, req.IsPrimary || address.IsPrimary,
            req.IsReturn || address.IsReturn, ct);
        address.UpdateInfo(req.WarehouseName, req.ContactName, req.ContactPhone, req.ProvinceId!.Value,
            req.ProvinceName, req.DistrictId!.Value, req.DistrictName, req.WardCode!, req.WardName, req.DetailAddress);
        return address.ToResponse();
    }
}
