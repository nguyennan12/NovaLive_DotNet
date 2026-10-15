using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Category?> GetByIdAsync(Guid id, bool includeInvisible, CancellationToken ct = default);

    Task<List<Category>> GetAllAsync(CancellationToken ct = default);

    Task<List<Category>> GetAllAsync(bool includeInvisible, CancellationToken ct = default);

    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<bool> HasChildrenAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);

    void Update(Category category);

    void Delete(Category category);
}
