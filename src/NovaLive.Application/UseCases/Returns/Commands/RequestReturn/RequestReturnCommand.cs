using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
public sealed record RequestReturnCommand(CreateReturnRequest Request) : ICommand<OrderReturnResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RequestOwn;
}
