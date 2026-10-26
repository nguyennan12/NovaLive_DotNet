using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
public sealed record ConfirmReturnReceivedCommand(Guid Id) : ICommand<OrderReturnResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RespondOwn;
}
