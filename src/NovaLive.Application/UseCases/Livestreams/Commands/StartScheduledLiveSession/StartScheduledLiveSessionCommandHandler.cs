using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.StartScheduledLiveSession;

public sealed class StartScheduledLiveSessionCommandHandler(
    ILivestreamSessionRepository sessions,
    IShopRepository shops,
    IAppDbContext db,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IAgoraTokenService agora,
    IDateTimeProvider clock)
    : ICommandHandler<StartScheduledLiveSessionCommand, LivestreamStartResponse>
{
    public async Task<Result<LivestreamStartResponse>> Handle(StartScheduledLiveSessionCommand command, CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        var hostId = currentUser.UserId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty || !hostId.HasValue || hostId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var session = await sessions.GetByIdAndShopAsync(command.SessionId, shopId.Value, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (await sessions.HasActiveLiveSessionAsync(shopId.Value, ct))
        {
            return LivestreamErrors.ShopAlreadyHasActiveLive;
        }

        var started = session.Start(clock.UtcNow);
        if (started.IsFailure)
        {
            return started.Error;
        }

        sessions.Update(session);
        await db.SaveChangesAsync(ct);
        var shop = await shops.GetActiveByIdAsync(shopId.Value, ct);
        if (shop is null)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var token = agora.GeneratePublisherToken(session.AgoraChannelName, hostId.Value.ToString());
        return new LivestreamStartResponse(
            session.Id,
            shop.Id,
            shop.ShopName,
            shop.LogoUrl,
            session.Title,
            session.ThumbnailUrl,
            session.AgoraChannelName,
            token,
            session.Status.ToString(),
            session.ViewerCount,
            session.StartedAt ?? clock.UtcNow);
    }
}
