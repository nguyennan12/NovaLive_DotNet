using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;

public record PinProductCommand(Guid SessionId, PinProductRequest Request)
    : ICommand<PinnedProductResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Livestreams.PinOwnProduct;
}
