using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscountsAdmin;

public record GetPlatformDiscountsAdminQuery(string? Status = null, int Page = 1, int Size = 20)
    : IQuery<PagedResult<DiscountResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Discounts.ManagePlatform;
}
