using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shipping.Commands.SimulateShipmentDelivered;

[RequireAuthenticated]
public sealed record SimulateShipmentDeliveredCommand(Guid SubOrderId)
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.FulfillmentOwn;
}
