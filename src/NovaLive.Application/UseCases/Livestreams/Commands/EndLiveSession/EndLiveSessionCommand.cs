using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Livestreams.Commands.EndLiveSession;

public record EndLiveSessionCommand(Guid SessionId)
    : ICommand<EndLiveSessionSummaryResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Livestreams.StartOwn;
}
