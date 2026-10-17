using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
public sealed record ApproveReturnCommand(Guid Id, SellerReturnResponseRequest Request) : ICommand<OrderReturnResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RespondOwn;
}
