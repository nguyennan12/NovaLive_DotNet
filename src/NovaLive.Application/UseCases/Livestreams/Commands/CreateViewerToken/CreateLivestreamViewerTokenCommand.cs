using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreateViewerToken;

public record CreateLivestreamViewerTokenCommand(Guid SessionId, JoinLiveSessionRequest? Request = null)
    : ICommand<LivestreamTokenResponse>;
