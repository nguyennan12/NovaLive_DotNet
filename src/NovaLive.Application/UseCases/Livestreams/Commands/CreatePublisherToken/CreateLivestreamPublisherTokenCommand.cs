using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Livestreams.Commands.CreatePublisherToken;

public record CreateLivestreamPublisherTokenCommand(Guid SessionId)
    : ICommand<LivestreamTokenResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Livestreams.StartOwn;
}
