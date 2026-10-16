using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Livestreams;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreateViewerToken;

public sealed class CreateLivestreamViewerTokenCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ICurrentUser currentUser,
    IAgoraTokenService agoraTokenService)
    : ICommandHandler<CreateLivestreamViewerTokenCommand, LivestreamTokenResponse>
{
    public async Task<Result<LivestreamTokenResponse>> Handle(
        CreateLivestreamViewerTokenCommand command,
        CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (session.Status != LivestreamStatus.Live)
        {
            return LivestreamErrors.NotLive;
        }

        string account;
        if (currentUser.UserId.HasValue && currentUser.UserId.Value != Guid.Empty)
        {
            account = currentUser.UserId.Value.ToString();
        }
        else if (!string.IsNullOrWhiteSpace(command.Request?.GuestId))
        {
            account = command.Request.GuestId.Trim();
        }
        else
        {
            account = $"guest_{Guid.NewGuid():N}";
        }

        var token = agoraTokenService.GenerateSubscriberToken(session.AgoraChannelName, account);

        return new LivestreamTokenResponse(
            session.Id,
            session.AgoraChannelName,
            token,
            account,
            "Subscriber",
            7200);
    }
}
