using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.StartScheduledLiveSession;

public sealed record StartScheduledLiveSessionCommand(Guid SessionId) : ICommand<LivestreamStartResponse>;
