using NovaLive.Domain.Discounts;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IDiscountRepository
{
    Task<bool> TryConsumeAsync(Guid discountId, Guid userId, DateTimeOffset now, CancellationToken ct);
    Task AddUsageAsync(DiscountUsage usage, CancellationToken ct);

    Task<bool> ExistsCodeAsync(string code, CancellationToken ct = default);

    Task<Discount?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Discount>> GetPublicShopDiscountsAsync(Guid shopId, DateTimeOffset now, CancellationToken ct = default);

    Task<IReadOnlyList<Discount>> GetPublicPlatformDiscountsAsync(DateTimeOffset now, CancellationToken ct = default);

    Task<(IReadOnlyList<Discount> Items, long TotalCount)> GetSellerDiscountsAsync(
        Guid shopId,
        string? status,
        int skip,
        int take,
        DateTimeOffset now,
        CancellationToken ct = default);

    Task AddAsync(Discount discount, CancellationToken ct = default);

    void Update(Discount discount);

    void Delete(Discount discount);

    Task<(IReadOnlyList<Discount> Items, long TotalCount)> GetPlatformDiscountsAdminAsync(
        string? status,
        int skip,
        int take,
        DateTimeOffset now,
        CancellationToken ct = default);

    Task<int> GetUserUsageCountAsync(Guid discountId, Guid userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
