using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
public sealed record RejectReturnCommand(Guid Id, RejectReturnRequest Request) : ICommand<OrderReturnResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RespondOwn;
}
