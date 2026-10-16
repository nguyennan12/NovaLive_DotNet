using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreatePublisherToken;

public sealed class CreateLivestreamPublisherTokenCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IAgoraTokenService agoraTokenService)
    : ICommandHandler<CreateLivestreamPublisherTokenCommand, LivestreamTokenResponse>
{
    public async Task<Result<LivestreamTokenResponse>> Handle(
        CreateLivestreamPublisherTokenCommand command,
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

        var session = await sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (session.ShopId != shopId.Value)
        {
            return LivestreamErrors.ForbiddenShop;
        }

        if (session.Status != LivestreamStatus.Live)
        {
            return LivestreamErrors.NotLive;
        }

        var account = currentUser.UserId.Value.ToString();
        var token = agoraTokenService.GeneratePublisherToken(session.AgoraChannelName, account);

        return new LivestreamTokenResponse(
            session.Id,
            session.AgoraChannelName,
            token,
            account,
            "Publisher",
            7200);
    }
}
