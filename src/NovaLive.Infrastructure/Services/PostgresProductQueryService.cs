using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Infrastructure.Services;

public sealed class PostgresProductQueryService(AppDbContext dbContext) : IProductQueryService
{
    public async Task<PagedResult<ProductSummaryResponse>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!dbContext.Database.IsNpgsql())
        {
            return await SearchFallbackAsync(request, cancellationToken);
        }

        var hasKeyword = !string.IsNullOrWhiteSpace(request.Keyword);
        var keyword = request.Keyword?.Trim() ?? string.Empty;

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.Size, 1, 100);
        var offset = (page - 1) * size;

        var orderByClause = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "price_asc" => "min_price ASC, created_at DESC",
            "price_desc" => "max_price DESC, created_at DESC",
            "best_selling" or "sold_desc" => hasKeyword ? "rank DESC, created_at DESC" : "created_at DESC",
            "newest" or "created_at_desc" => "created_at DESC",
            _ => hasKeyword ? "rank DESC, created_at DESC" : "created_at DESC"
        };

        var sql = $@"
WITH filtered_spus AS (
    SELECT 
        s.id AS spu_id,
        s.shop_id,
        sh.shop_name,
        s.category_id,
        c.name AS category_name,
        s.name,
        s.thumbnail_url,
        s.created_at,
        MIN(sk.sell_price) AS min_price,
        MAX(sk.sell_price) AS max_price,
        CASE 
            WHEN @hasKeyword::boolean THEN ts_rank(COALESCE(s.search_vector, to_tsvector('vietnamese', s.name)), websearch_to_tsquery('vietnamese', @keyword))
            ELSE 0.0
        END AS rank
    FROM spus s
    JOIN shops sh ON s.shop_id = sh.id
    JOIN categories c ON s.category_id = c.id
    JOIN skus sk ON sk.spu_id = s.id AND sk.is_active = TRUE AND sk.deleted_at IS NULL
    WHERE s.status = 'Active' 
      AND s.deleted_at IS NULL 
      AND sh.status = 'Active' 
      AND sh.deleted_at IS NULL
      AND (@shopId::uuid IS NULL OR s.shop_id = @shopId)
      AND (@categoryId::uuid IS NULL OR s.category_id = @categoryId)
      AND (
          @hasKeyword::boolean = FALSE OR (
              (s.search_vector IS NOT NULL AND s.search_vector @@ websearch_to_tsquery('vietnamese', @keyword))
              OR s.name % @keyword
          )
      )
    GROUP BY s.id, s.shop_id, sh.shop_name, s.category_id, c.name, s.name, s.thumbnail_url, s.created_at, s.search_vector
    HAVING 
        (@minPrice::numeric IS NULL OR MAX(sk.sell_price) >= @minPrice)
        AND (@maxPrice::numeric IS NULL OR MIN(sk.sell_price) <= @maxPrice)
)
SELECT 
    spu_id,
    shop_id,
    shop_name,
    category_id,
    category_name,
    name,
    thumbnail_url,
    min_price,
    max_price,
    rank,
    COUNT(*) OVER() AS total_count
FROM filtered_spus
ORDER BY {orderByClause}
LIMIT @pageSize OFFSET @offset;";

        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        void AddParam(string name, object? value, DbType? dbType = null)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            if (dbType.HasValue)
            {
                p.DbType = dbType.Value;
            }
            command.Parameters.Add(p);
        }

        AddParam("hasKeyword", hasKeyword, DbType.Boolean);
        AddParam("keyword", keyword, DbType.String);
        AddParam("shopId", request.ShopId, DbType.Guid);
        AddParam("categoryId", request.CategoryId, DbType.Guid);
        AddParam("minPrice", request.MinPrice, DbType.Decimal);
        AddParam("maxPrice", request.MaxPrice, DbType.Decimal);
        AddParam("pageSize", size, DbType.Int32);
        AddParam("offset", offset, DbType.Int32);

        var items = new List<ProductSummaryResponse>();
        long totalCount = 0;

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            totalCount = reader.GetInt64(reader.GetOrdinal("total_count"));
            var spuId = reader.GetGuid(reader.GetOrdinal("spu_id"));
            var shopId = reader.GetGuid(reader.GetOrdinal("shop_id"));
            var shopName = reader.GetString(reader.GetOrdinal("shop_name"));
            var categoryId = reader.GetGuid(reader.GetOrdinal("category_id"));
            var categoryName = reader.GetString(reader.GetOrdinal("category_name"));
            var name = reader.GetString(reader.GetOrdinal("name"));
            var thumbnailUrl = reader.IsDBNull(reader.GetOrdinal("thumbnail_url"))
                ? null
                : reader.GetString(reader.GetOrdinal("thumbnail_url"));
            var minPrice = reader.GetDecimal(reader.GetOrdinal("min_price"));
            var maxPrice = reader.GetDecimal(reader.GetOrdinal("max_price"));
            var rank = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("rank")));

            items.Add(new ProductSummaryResponse(
                Id: spuId,
                ShopId: shopId,
                ShopName: shopName,
                CategoryId: categoryId,
                CategoryName: categoryName,
                Name: name,
                ThumbnailUrl: thumbnailUrl,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                Rating: 5.0,
                SoldCount: 0,
                SearchRank: Math.Round(rank, 4)));
        }

        return PagedResult<ProductSummaryResponse>.Create(items, page, size, totalCount);
    }

    private async Task<PagedResult<ProductSummaryResponse>> SearchFallbackAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        var query =
            from spu in dbContext.Spus.AsNoTracking()
            join shop in dbContext.Shops.AsNoTracking() on spu.ShopId equals shop.Id
            join category in dbContext.Categories.AsNoTracking() on spu.CategoryId equals category.Id
            join sku in dbContext.Skus.AsNoTracking() on spu.Id equals sku.SpuId
            where spu.Status == ProductStatus.Active
                && spu.DeletedAt == null
                && shop.Status == ShopStatus.Active
                && shop.DeletedAt == null
                && sku.IsActive
                && sku.DeletedAt == null
            select new { Spu = spu, Shop = shop, Category = category, Sku = sku };

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
            var kw = request.Keyword.Trim();
            query = query.Where(item => EF.Functions.Like(item.Spu.Name, $"%{kw}%")
                || (item.Spu.Description != null && EF.Functions.Like(item.Spu.Description, $"%{kw}%")));
        }

        if (request.MinPrice is { } minPrice)
        {
            query = query.Where(item => item.Sku.SellPrice >= minPrice);
        }

        if (request.MaxPrice is { } maxPrice)
        {
            query = query.Where(item => item.Sku.SellPrice <= maxPrice);
        }

        var grouped = query.GroupBy(item => new
        {
            item.Spu.Id,
            item.Spu.ShopId,
            item.Shop.ShopName,
            item.Spu.CategoryId,
            item.Category.Name,
            SpuName = item.Spu.Name,
            item.Spu.ThumbnailUrl,
            item.Spu.CreatedAt
        })
        .Select(g => new
        {
            g.Key.Id,
            g.Key.ShopId,
            g.Key.ShopName,
            g.Key.CategoryId,
            CategoryName = g.Key.Name,
            Name = g.Key.SpuName,
            g.Key.ThumbnailUrl,
            g.Key.CreatedAt,
            MinPrice = g.Min(item => item.Sku.SellPrice),
            MaxPrice = g.Max(item => item.Sku.SellPrice)
        });

        var allItems = await grouped.ToListAsync(cancellationToken);
        var total = allItems.Count;

        var ordered = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "price_asc" => allItems.OrderBy(x => x.MinPrice).ThenByDescending(x => x.CreatedAt),
            "price_desc" => allItems.OrderByDescending(x => x.MaxPrice).ThenByDescending(x => x.CreatedAt),
            _ => allItems.OrderByDescending(x => x.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.Size, 1, 100);

        var paged = ordered
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var items = paged.Select(x => new ProductSummaryResponse(
            Id: x.Id,
            ShopId: x.ShopId,
            ShopName: x.ShopName,
            CategoryId: x.CategoryId,
            CategoryName: x.CategoryName,
            Name: x.Name,
            ThumbnailUrl: x.ThumbnailUrl,
            MinPrice: x.MinPrice,
            MaxPrice: x.MaxPrice,
            Rating: 5.0,
            SoldCount: 0,
            SearchRank: 0m)).ToList();

        return PagedResult<ProductSummaryResponse>.Create(items, page, size, total);
    }
}
