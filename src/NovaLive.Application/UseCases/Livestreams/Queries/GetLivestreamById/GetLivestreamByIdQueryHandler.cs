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

        var products = new List<PinnedProductResponse>();
        var sessionProducts = await productRepository.GetBySessionIdAsync(session.Id, ct);
        foreach (var liveProduct in sessionProducts)
        {
            var sku = await skuRepository.GetByIdAsync(liveProduct.SkuId, ct);
            if (sku is not null)
            {
                var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);
                var inv = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, session.ShopId, ct);
                if (spu is not null)
                {
                    products.Add(liveProduct.ToPinnedResponse(sku, spu, inv?.AvailableQty ?? 0));
                }
            }
        }

        var activePinnedProduct = products.FirstOrDefault(product => product.IsPinned);
        return session.ToDetailResponse(shop, activePinnedProduct, products);
    }
}
