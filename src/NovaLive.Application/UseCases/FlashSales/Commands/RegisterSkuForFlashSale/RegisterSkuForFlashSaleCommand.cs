using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;

public record RegisterSkuForFlashSaleCommand(Guid CampaignId, RegisterFlashSaleSkuRequest Request)
    : ICommand<FlashSaleItemResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.FlashSales.RegisterOwn;
}
