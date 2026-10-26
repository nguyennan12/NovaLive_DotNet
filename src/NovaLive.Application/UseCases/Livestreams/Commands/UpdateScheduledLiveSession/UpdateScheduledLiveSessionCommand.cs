using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.UpdateScheduledLiveSession;

public sealed record UpdateScheduledLiveSessionCommand(Guid SessionId, UpdateScheduledLivestreamRequest Request) : ICommand;
