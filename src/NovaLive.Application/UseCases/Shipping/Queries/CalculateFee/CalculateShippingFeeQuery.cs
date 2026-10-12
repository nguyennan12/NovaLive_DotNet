using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;

public record CalculateShippingFeeQuery(CalculateShippingFeeRequest Request)
    : IQuery<ShippingFeeResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Shipping.CalculateFee;
}
