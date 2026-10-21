using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Infrastructure.Search;

public sealed class PostgresProductSearchIndexer(AppDbContext dbContext) : IProductSearchIndexer
{
    public Task<int> RebuildSearchVectorAsync(Guid spuId, CancellationToken cancellationToken)
    {
        // Recompute from current DB state: duplicate/out-of-order events are safe without Redis idempotency.
        // This read-model update leaves updated_at unchanged because product content has not changed.
        // Raw SQL bypasses soft-delete interceptors, so the live-row filter is mandatory.
        return dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE spus
            SET search_vector = to_tsvector({PostgresSearchConfiguration.TextSearchConfig}::regconfig,
                concat_ws(' ', name, description, brand))
            WHERE id = {spuId} AND deleted_at IS NULL
            """, cancellationToken);
    }
}
