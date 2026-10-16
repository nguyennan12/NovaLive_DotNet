using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;

[RequireAuthenticated]
public sealed record CalculateCheckoutDraftQuery(CalculateCheckoutRequest Request)
    : IQuery<CheckoutPreviewResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Orders.Checkout;
}
