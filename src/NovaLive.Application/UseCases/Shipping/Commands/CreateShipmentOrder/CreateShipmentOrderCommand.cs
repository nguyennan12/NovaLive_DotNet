using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;

[RequireAuthenticated]
public sealed record CreateShipmentOrderCommand(CreateShippingOrderRequest Request)
    : ICommand<ShippingOrderResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.FulfillmentOwn;
}
