using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Livestreams;
using NovaLive.Domain.Products;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Livestreams;

public static class LivestreamMappings
{
    public static LivestreamSessionResponse ToResponse(this LivestreamSession session, Shop? shop = null)
    {
        return new LivestreamSessionResponse(
            session.Id,
            session.ShopId,
            shop?.ShopName ?? string.Empty,
            shop?.LogoUrl,
            session.Title,
            session.ThumbnailUrl,
            session.AgoraChannelName,
            session.Status.ToString(),
            session.ViewerCount,
            session.PeakViewerCount,
            session.StartedAt,
            session.EndedAt);
    }

    public static LivestreamStartResponse ToStartResponse(
        this LivestreamSession session,
        string publisherToken,
        Shop? shop = null)
    {
        return new LivestreamStartResponse(
            session.Id,
            session.ShopId,
            shop?.ShopName ?? string.Empty,
            shop?.LogoUrl,
            session.Title,
            session.ThumbnailUrl,
            session.AgoraChannelName,
            publisherToken,
            session.Status.ToString(),
            session.ViewerCount,
            session.StartedAt ?? session.CreatedAt);
    }

    public static LivestreamSessionDetailResponse ToDetailResponse(
        this LivestreamSession session,
        Shop? shop,
        PinnedProductResponse? activePinnedProduct)
    {
        return new LivestreamSessionDetailResponse(
            session.Id,
            session.ShopId,
            shop?.ShopName ?? string.Empty,
            shop?.LogoUrl,
            session.Title,
            session.ThumbnailUrl,
            session.AgoraChannelName,
            session.Status.ToString(),
            session.ViewerCount,
            session.PeakViewerCount,
            session.StartedAt,
            session.EndedAt,
            activePinnedProduct);
    }

    public static PinnedProductResponse ToPinnedResponse(
        this LivestreamProduct liveProduct,
        Sku sku,
        Spu spu,
        int availableQty,
        int durationSeconds = 120)
    {
        var effectivePrice = liveProduct.FlashPrice ?? sku.SellPrice;
        DateTimeOffset? expiresAt = liveProduct.PinnedAt?.AddSeconds(durationSeconds);

        return new PinnedProductResponse(
            liveProduct.SessionId,
            liveProduct.SkuId,
            spu.Id,
            spu.Name,
            sku.SkuCode,
            sku.AttributesJson,
            spu.ThumbnailUrl,
            sku.OriginalPrice,
            sku.SellPrice,
            liveProduct.FlashPrice,
            effectivePrice,
            liveProduct.QuantityLimit,
            liveProduct.SoldInLive,
            liveProduct.IsPinned,
            liveProduct.PinnedAt,
            expiresAt,
            availableQty);
    }

    public static LivestreamCommentResponse ToResponse(this LivestreamComment comment)
    {
        return new LivestreamCommentResponse(
            comment.Id,
            comment.SessionId,
            comment.UserId,
            comment.DisplayName,
            comment.Content,
            comment.IsQuestion,
            comment.CreatedAt);
    }
}

