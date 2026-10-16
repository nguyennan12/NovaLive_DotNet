using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
[RequireAuthenticated]
public sealed record CancelSubOrderCommand(Guid SubOrderId, CancelOrderRequest Request)
    : ICommand<OrderOperationResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.CancelOwnSell;
}
