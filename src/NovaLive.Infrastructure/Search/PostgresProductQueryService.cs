using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Contracts.Products;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Infrastructure.Search;

public sealed class PostgresProductQueryService(AppDbContext dbContext) : IProductQueryService
{
    public async Task<IReadOnlyList<ProductSummaryDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query =
            from spu in dbContext.Spus.AsNoTracking()
            join sku in dbContext.Skus.AsNoTracking() on spu.Id equals sku.SpuId
            where spu.Status == ProductStatus.Active
                && spu.DeletedAt == null
                && sku.IsActive
                && sku.DeletedAt == null
            select new { Spu = spu, Sku = sku };

        if (request.ShopId is { } shopId)
        {
            query = query.Where(item => item.Spu.ShopId == shopId);
        }

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(item => item.Spu.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(item => EF.Functions.ILike(item.Spu.Name, $"%{request.Keyword}%"));
        }

        if (request.MinPrice is { } minPrice)
        {
            query = query.Where(item => item.Sku.SellPrice >= minPrice);
        }

        if (request.MaxPrice is { } maxPrice)
        {
            query = query.Where(item => item.Sku.SellPrice <= maxPrice);
        }

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.Size, 1, 100);

        return await query
            .GroupBy(item => new
            {
                item.Spu.Id,
                item.Spu.ShopId,
                item.Spu.Name,
                item.Spu.ThumbnailUrl
            })
            .Select(group => new ProductSummaryDto(
                group.Key.Id,
                group.Key.ShopId,
                group.Key.Name,
                group.Key.ThumbnailUrl,
                group.Min(item => item.Sku.SellPrice),
                group.Max(item => item.Sku.SellPrice),
                0))
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);
    }
}
