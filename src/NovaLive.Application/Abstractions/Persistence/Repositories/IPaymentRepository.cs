using NovaLive.Domain.Payments;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Repository quản lý truy vấn và lưu trữ thực thể Payment.
/// </summary>
public interface IPaymentRepository
{
    Task<List<Payment>> GetByParentIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);

    Task<Payment?> GetForUpdateAsync(Guid parentId, CancellationToken ct);

    Task<Payment?> GetByTransactionRefAsync(string transactionRef, CancellationToken ct);

    Task<Payment?> GetByParentOrderIdAsync(Guid parentOrderId, CancellationToken ct);

    Task AddAsync(Payment payment, CancellationToken ct);
}
