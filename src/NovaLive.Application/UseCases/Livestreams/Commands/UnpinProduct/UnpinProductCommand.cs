using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Livestreams.Commands.UnpinProduct;

public record UnpinProductCommand(Guid SessionId, Guid? SkuId = null)
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Livestreams.PinOwnProduct;
}
