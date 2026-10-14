using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Products;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(IAppDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => GetByIdAsync(id, includeInvisible: false, ct);

    public async Task<Category?> GetByIdAsync(Guid id, bool includeInvisible, CancellationToken ct = default)
    {
        var query = dbContext.Categories.AsQueryable();
        if (!includeInvisible)
        {
            query = query.Where(c => c.IsVisible);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        => GetAllAsync(includeInvisible: false, ct);

    public async Task<List<Category>> GetAllAsync(bool includeInvisible, CancellationToken ct = default)
    {
        var query = dbContext.Categories.AsNoTracking();
        if (!includeInvisible)
        {
            query = query.Where(c => c.IsVisible);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, ct);
    }

    public async Task<bool> HasChildrenAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.ParentId == id, ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await dbContext.Categories.AddAsync(category, ct);
    }

    public void Update(Category category)
    {
        dbContext.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        dbContext.Categories.Remove(category);
    }
}
