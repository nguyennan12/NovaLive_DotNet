using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.ScheduleLiveSession;

public sealed record ScheduleLiveSessionCommand(ScheduleLivestreamRequest Request)
    : ICommand<ScheduledLivestreamResponse>;
