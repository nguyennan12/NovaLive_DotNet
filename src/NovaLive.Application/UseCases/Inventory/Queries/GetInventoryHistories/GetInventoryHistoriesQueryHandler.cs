using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Inventory;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory.Queries.GetInventoryHistories;

public sealed class GetInventoryHistoriesQueryHandler(
    IInventoryRepository inventoryRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetInventoryHistoriesQuery, PagedResult<InventoryHistoryResponse>>
{
    public async Task<Result<PagedResult<InventoryHistoryResponse>>> Handle(GetInventoryHistoriesQuery query, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return InventoryErrors.UnauthorizedShop;
        }

        var req = query.Request;

        var (items, total) = await inventoryRepository.GetHistoriesPagedAsync(
            shopId: shopId.Value,
            skuId: req.SkuId,
            changeType: req.ChangeType,
            from: req.From,
            to: req.To,
            page: req.NormalizedPage,
            size: req.NormalizedSize,
            ct: ct);

        var responses = items.Select(x => x.History.ToHistoryResponse(x.SkuCode, x.SpuName)).ToList();

        return PagedResult<InventoryHistoryResponse>.Create(responses, req.NormalizedPage, req.NormalizedSize, total);
    }
}
