using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory.Queries.GetSellerInventory;

public sealed class GetSellerInventoryQueryHandler(
    IAppDbContext dbContext,
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
        var page = req.Page <= 0 ? 1 : req.Page;
        var size = req.Size <= 0 ? 20 : (req.Size > 100 ? 100 : req.Size);

        var baseQuery = from inventory in dbContext.Inventories.AsNoTracking()
                        join sku in dbContext.Skus.AsNoTracking() on inventory.SkuId equals sku.Id
                        join spu in dbContext.Spus.AsNoTracking() on sku.SpuId equals spu.Id
                        where inventory.ShopId == shopId.Value
                            && sku.DeletedAt == null
                            && spu.DeletedAt == null
                        select new
                        {
                            Inventory = inventory,
                            Sku = sku,
                            Spu = spu
                        };

        if (req.LowStock == true)
        {
            baseQuery = baseQuery.Where(x => (x.Inventory.QtyOnHand - x.Inventory.ReservedQty) <= x.Inventory.MinStock);
        }

        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var keyword = req.Keyword.Trim().ToLower();
            baseQuery = baseQuery.Where(x => x.Sku.SkuCode.ToLower().Contains(keyword) || x.Spu.Name.ToLower().Contains(keyword));
        }

        var total = await baseQuery.LongCountAsync(ct);

        var pagedItems = await baseQuery
            .OrderByDescending(x => x.Inventory.LastUpdated)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var responses = pagedItems.Select(x => x.Inventory.ToResponse(x.Sku.SkuCode, x.Spu.Name)).ToList();

        return PagedResult<InventoryResponse>.Create(responses, page, size, total);
    }
}
