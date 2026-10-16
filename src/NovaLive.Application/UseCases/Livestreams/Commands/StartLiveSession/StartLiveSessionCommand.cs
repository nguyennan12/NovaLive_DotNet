using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;

public record StartLiveSessionCommand(StartLivestreamRequest Request)
    : ICommand<LivestreamStartResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Livestreams.StartOwn;
}
