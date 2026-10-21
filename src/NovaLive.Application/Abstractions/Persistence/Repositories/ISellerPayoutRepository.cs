using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface ISellerPayoutRepository
{
    Task AddAsync(SellerPayout payout, CancellationToken ct = default);

    Task<SellerPayout?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<SellerPayout?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<(List<SellerPayout> Items, long TotalCount)> GetPagedAsync(
        Guid? shopId,
        PayoutStatus? status,
        int page,
        int size,
        CancellationToken ct = default);
}
