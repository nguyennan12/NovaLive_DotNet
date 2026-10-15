using NovaLive.Domain.Payments;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IPaymentRepository
{
    Task<List<Payment>> GetByParentIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<Payment?> GetForUpdateAsync(Guid parentId, CancellationToken ct);

    Task AddAsync(Payment payment, CancellationToken ct);
}
