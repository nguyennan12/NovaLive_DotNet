using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;

public sealed class UpdateShopInfoCommandHandler(IShopRepository shops, ShopAccess access) : ICommandHandler<UpdateShopInfoCommand, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(UpdateShopInfoCommand command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(true, ct);
        if (owned.IsFailure) return owned.Error;
        var shop = owned.Value!;
        var req = command.Request;
        var result = shop.UpdateInfo(req.ShopName, req.Description, req.LogoUrl, req.BannerUrl, req.Phone, req.Email);
        if (result.IsFailure) return result.Error;
        return shop.ToResponse(await shops.CountFollowersAsync(shop.Id, ct));
    }
}
