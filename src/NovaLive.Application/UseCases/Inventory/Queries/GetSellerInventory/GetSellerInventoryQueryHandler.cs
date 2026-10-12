using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Inventory;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory.Queries.GetSellerInventory;

public sealed class GetSellerInventoryQueryHandler(
    IInventoryRepository inventoryRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetSellerInventoryQuery, PagedResult<InventoryResponse>>
{
    public async Task<Result<PagedResult<InventoryResponse>>> Handle(GetSellerInventoryQuery query, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return InventoryErrors.UnauthorizedShop;
        }

        var req = query.Request;

        var (items, total) = await inventoryRepository.GetSellerInventoryPagedAsync(
            shopId: shopId.Value,
            lowStock: req.LowStock,
            keyword: req.Keyword,
            page: req.NormalizedPage,
            size: req.NormalizedSize,
            ct: ct);

        var responses = items.Select(x => x.Inventory.ToResponse(x.Sku.SkuCode, x.Spu.Name)).ToList();

        return PagedResult<InventoryResponse>.Create(responses, req.NormalizedPage, req.NormalizedSize, total);
    }
}
