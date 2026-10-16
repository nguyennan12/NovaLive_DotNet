using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
[RequireAuthenticated]
public sealed record ConfirmSubOrderCommand(Guid SubOrderId, ConfirmSubOrderRequest Request)
    : ICommand<OrderOperationResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.FulfillmentOwn;
}
