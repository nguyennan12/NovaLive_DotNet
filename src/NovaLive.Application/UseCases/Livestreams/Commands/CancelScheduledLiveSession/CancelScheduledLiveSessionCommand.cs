using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CancelScheduledLiveSession;

public sealed record CancelScheduledLiveSessionCommand(Guid SessionId) : ICommand;
