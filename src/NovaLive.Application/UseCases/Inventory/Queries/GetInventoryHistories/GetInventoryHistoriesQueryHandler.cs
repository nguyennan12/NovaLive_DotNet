using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory.Queries.GetInventoryHistories;

public sealed class GetInventoryHistoriesQueryHandler(
    IAppDbContext dbContext,
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
        var page = req.Page <= 0 ? 1 : req.Page;
        var size = req.Size <= 0 ? 20 : (req.Size > 100 ? 100 : req.Size);

        var baseQuery = from history in dbContext.InventoryHistories.AsNoTracking()
                        join sku in dbContext.Skus.AsNoTracking() on history.SkuId equals sku.Id
                        join spu in dbContext.Spus.AsNoTracking() on sku.SpuId equals spu.Id
                        where sku.ShopId == shopId.Value
                        select new
                        {
                            History = history,
                            Sku = sku,
                            Spu = spu
                        };

        if (req.SkuId.HasValue && req.SkuId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(x => x.History.SkuId == req.SkuId.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.ChangeType) &&
            Enum.TryParse<InventoryChangeType>(req.ChangeType, ignoreCase: true, out var changeType))
        {
            baseQuery = baseQuery.Where(x => x.History.ChangeType == changeType);
        }

        if (req.From.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.History.CreatedAt >= req.From.Value);
        }

        if (req.To.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.History.CreatedAt <= req.To.Value);
        }

        var total = await baseQuery.LongCountAsync(ct);

        var pagedItems = await baseQuery
            .OrderByDescending(x => x.History.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var responses = pagedItems.Select(x => x.History.ToHistoryResponse(x.Sku.SkuCode, x.Spu.Name)).ToList();

        return PagedResult<InventoryHistoryResponse>.Create(responses, page, size, total);
    }
}
