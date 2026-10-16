using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetLivestreamById;

public sealed class GetLivestreamByIdQueryHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamProductRepository productRepository,
    IShopRepository shopRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    IInventoryRepository inventoryRepository)
    : IQueryHandler<GetLivestreamByIdQuery, LivestreamSessionDetailResponse>
{
    public async Task<Result<LivestreamSessionDetailResponse>> Handle(
        GetLivestreamByIdQuery query,
        CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(query.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        var shop = await shopRepository.GetActiveByIdAsync(session.ShopId, ct);

        PinnedProductResponse? activePinnedProduct = null;
        var pinned = await productRepository.GetActivePinnedProductAsync(session.Id, ct);
        if (pinned is not null)
        {
            var sku = await skuRepository.GetByIdAsync(pinned.SkuId, ct);
            if (sku is not null)
            {
                var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);
                var inv = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, session.ShopId, ct);
                if (spu is not null)
                {
                    activePinnedProduct = pinned.ToPinnedResponse(sku, spu, inv?.AvailableQty ?? 0);
                }
            }
        }

        return session.ToDetailResponse(shop, activePinnedProduct);
    }
}
