using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
[RequireAuthenticated]
public sealed record CancelOrderCommand(Guid OrderId, CancelOrderRequest Request)
    : ICommand<OrderOperationResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.CancelOwnBuy;
}
