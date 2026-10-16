using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;

public sealed class EndLiveSessionCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamProductRepository productRepository,
    ISkuRepository skuRepository,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IDateTimeProvider dateTimeProvider,
    ILivestreamNotificationService notificationService,
    IAfterCommitActions afterCommit)
    : ICommandHandler<EndLiveSessionCommand, EndLiveSessionSummaryResponse>
{
    public async Task<Result<EndLiveSessionSummaryResponse>> Handle(
        EndLiveSessionCommand command,
        CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var session = await sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (session.ShopId != shopId.Value)
        {
            return LivestreamErrors.ForbiddenShop;
        }

        var sessionProducts = await productRepository.GetBySessionIdAsync(session.Id, ct);

        if (session.Status == LivestreamStatus.Ended)
        {
            return await BuildSummaryResponseAsync(session, sessionProducts, ct);
        }

        if (session.Status != LivestreamStatus.Live)
        {
            return LivestreamErrors.NotLive;
        }

        var now = dateTimeProvider.UtcNow;
        var endResult = session.End(now);
        if (endResult.IsFailure)
        {
            return endResult.Error;
        }

        sessionRepository.Update(session);

        var pinnedProducts = sessionProducts.Where(p => p.IsPinned).ToList();
        if (pinnedProducts.Count > 0)
        {
            foreach (var pinned in pinnedProducts)
            {
                pinned.Unpin();
            }
            productRepository.UpdateRange(pinnedProducts);
        }

        afterCommit.Add(async token => await notificationService.BroadcastLiveEndedAsync(session.Id, token));

        return await BuildSummaryResponseAsync(session, sessionProducts, ct);
    }

    private async Task<EndLiveSessionSummaryResponse> BuildSummaryResponseAsync(
        LivestreamSession session,
        List<LivestreamProduct> sessionProducts,
        CancellationToken ct)
    {
        var startedAt = session.StartedAt ?? session.CreatedAt;
        var endedAt = session.EndedAt ?? dateTimeProvider.UtcNow;
        var duration = endedAt > startedAt ? endedAt - startedAt : TimeSpan.Zero;

        var totalProductsSold = sessionProducts.Sum(p => p.SoldInLive);

        var soldProducts = sessionProducts.Where(p => p.SoldInLive > 0).ToList();
        var skuIds = soldProducts.Select(p => p.SkuId).Distinct().ToList();

        var skus = skuIds.Count > 0
            ? await skuRepository.GetByIdsAsync(skuIds, ct)
            : [];

        var skuMap = skus.ToDictionary(s => s.Id);

        decimal totalRevenue = 0m;
        foreach (var p in soldProducts)
        {
            var unitPrice = p.FlashPrice ?? (skuMap.TryGetValue(p.SkuId, out var sku) ? sku.SellPrice : 0m);
            totalRevenue += unitPrice * p.SoldInLive;
        }

        return new EndLiveSessionSummaryResponse(
            session.Id,
            session.Title,
            duration,
            session.PeakViewerCount,
            totalProductsSold,
            totalRevenue,
            startedAt,
            endedAt);
    }
}
