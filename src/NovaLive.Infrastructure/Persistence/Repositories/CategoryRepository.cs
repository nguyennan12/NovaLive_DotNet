using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(IAppDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.IsVisible, ct);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && c.IsVisible, ct);
    }
}
