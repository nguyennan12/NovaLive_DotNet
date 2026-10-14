using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetSellerFlashSaleRegistrations;

public record GetSellerFlashSaleRegistrationsQuery(
    Guid? CampaignId = null,
    string? Status = null,
    int Page = 1,
    int Size = 20)
    : IQuery<PagedResult<FlashSaleItemResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.FlashSales.RegisterOwn;
}
