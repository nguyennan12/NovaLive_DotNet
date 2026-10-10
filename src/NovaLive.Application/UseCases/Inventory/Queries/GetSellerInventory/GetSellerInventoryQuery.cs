using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Inventory;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Inventory.Queries.GetSellerInventory;

public record GetSellerInventoryQuery(GetSellerInventoryRequest Request)
    : IQuery<PagedResult<InventoryResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Inventory.ManageOwn;
}
