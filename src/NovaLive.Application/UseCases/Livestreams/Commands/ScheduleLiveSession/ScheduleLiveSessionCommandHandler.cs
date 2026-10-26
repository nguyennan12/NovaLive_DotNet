using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.ScheduleLiveSession;

public sealed class ScheduleLiveSessionCommandHandler(
    ILivestreamSessionRepository sessions,
    IShopRepository shops,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IDateTimeProvider clock)
    : ICommandHandler<ScheduleLiveSessionCommand, ScheduledLivestreamResponse>
{
    public async Task<Result<ScheduledLivestreamResponse>> Handle(ScheduleLiveSessionCommand command, CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        var hostId = currentUser.UserId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty || !hostId.HasValue || hostId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var request = command.Request;
        var now = clock.UtcNow;
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 3 || request.ScheduledAt <= now)
        {
            return LivestreamErrors.InvalidSchedule;
        }

        var shop = await shops.GetActiveByIdAsync(shopId.Value, ct);
        if (shop is null)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var id = Guid.NewGuid();
        var channelName = $"live_{id:N}";
        var session = new LivestreamSession(
            shopId.Value,
            hostId.Value,
            request.Title.Trim(),
            request.ThumbnailUrl?.Trim(),
            channelName,
            LivestreamStatus.Scheduled,
            request.ScheduledAt);
        await sessions.AddAsync(session, ct);

        return new ScheduledLivestreamResponse(
            session.Id,
            session.Title,
            session.ThumbnailUrl,
            session.AgoraChannelName,
            session.Status.ToString(),
            request.ScheduledAt);
    }
}
