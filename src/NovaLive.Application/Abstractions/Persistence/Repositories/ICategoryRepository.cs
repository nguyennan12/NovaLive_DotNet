using NovaLive.Domain.Products;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
