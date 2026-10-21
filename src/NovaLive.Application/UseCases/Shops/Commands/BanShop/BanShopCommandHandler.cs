using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.BanShop;

public sealed class BanShopCommandHandler(
    IShopRepository shops,
    ISpuRepository spus) : ICommandHandler<BanShopCommand, ShopResponse>
{
    public async Task<Result<ShopResponse>> Handle(BanShopCommand command, CancellationToken ct)
    {
        var shop = await shops.GetTrackedByIdAsync(command.ShopId, ct);
        if (shop is null)
        {
            return ShopErrors.NotFound;
        }

        shop.Ban();

        // Automatically set all SPUs to Inactive to hide from public search and prevent new orders
        await spus.DeactivateByShopIdAsync(shop.Id, ct);

        var verification = await shops.GetLatestVerificationAsync(shop.Id, ct);
        var followersCount = await shops.CountFollowersAsync(shop.Id, ct);

        return shop.ToResponse(followersCount, verification);
    }
}
