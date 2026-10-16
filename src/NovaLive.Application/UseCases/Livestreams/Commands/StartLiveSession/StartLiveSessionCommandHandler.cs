using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;

public sealed class StartLiveSessionCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    IShopRepository shopRepository,
    IShopAddressRepository shopAddressRepository,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IAgoraTokenService agoraTokenService,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<StartLiveSessionCommand, LivestreamStartResponse>
{
    public async Task<Result<LivestreamStartResponse>> Handle(
        StartLiveSessionCommand command,
        CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        if (!currentUser.UserId.HasValue || currentUser.UserId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var shop = await shopRepository.GetActiveByIdAsync(shopId.Value, ct);
        if (shop is null)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        if (command.Request.WarehouseAddressId.HasValue)
        {
            var warehouse = await shopAddressRepository.FindForShopAsync(
                command.Request.WarehouseAddressId.Value,
                shopId.Value,
                ct);

            if (warehouse is null)
            {
                return LivestreamErrors.WarehouseAddressNotFound;
            }
        }

        var hasActiveSession = await sessionRepository.HasActiveLiveSessionAsync(shopId.Value, ct);
        if (hasActiveSession)
        {
            return LivestreamErrors.ShopAlreadyHasActiveLive;
        }

        var sessionId = Guid.NewGuid();
        var channelName = $"live_{sessionId:N}";
        var now = dateTimeProvider.UtcNow;

        var publisherToken = agoraTokenService.GeneratePublisherToken(
            channelName,
            currentUser.UserId.Value.ToString());

        var session = new LivestreamSession(
            shopId: shopId.Value,
            hostId: currentUser.UserId.Value,
            title: command.Request.Title.Trim(),
            thumbnailUrl: command.Request.ThumbnailUrl?.Trim(),
            agoraChannelName: channelName,
            status: LivestreamStatus.Live,
            startedAt: now,
            id: sessionId);

        await sessionRepository.AddAsync(session, ct);

        return session.ToStartResponse(publisherToken, shop);
    }
}
