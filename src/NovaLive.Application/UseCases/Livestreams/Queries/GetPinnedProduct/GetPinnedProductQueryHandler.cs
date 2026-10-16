using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Livestreams.Queries.GetPinnedProduct;

public sealed class GetPinnedProductQueryHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamProductRepository productRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    IInventoryRepository inventoryRepository)
    : IQueryHandler<GetPinnedProductQuery, PinnedProductResponse>
{
    public async Task<Result<PinnedProductResponse>> Handle(
        GetPinnedProductQuery query,
        CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(query.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        var pinned = await productRepository.GetActivePinnedProductAsync(session.Id, ct);
        if (pinned is null)
        {
            return LivestreamErrors.ProductNotPinned;
        }

        var sku = await skuRepository.GetByIdAsync(pinned.SkuId, ct);
        if (sku is null)
        {
            return LivestreamErrors.SkuNotFound;
        }

        var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);
        if (spu is null)
        {
            return LivestreamErrors.SkuNotFound;
        }

        var inventory = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, session.ShopId, ct);
        var availableQty = inventory?.AvailableQty ?? 0;

        return pinned.ToPinnedResponse(sku, spu, availableQty);
    }
}
