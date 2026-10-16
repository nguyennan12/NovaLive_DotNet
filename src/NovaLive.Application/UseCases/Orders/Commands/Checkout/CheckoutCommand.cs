using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Orders.Commands.Checkout;
[RequireAuthenticated]
public sealed record CheckoutCommand(CheckoutRequest Request, string IdempotencyKey)
    : ICommand<CheckoutResponse>, IManagesTransactionCommand, IIdempotentCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.Checkout;
}
