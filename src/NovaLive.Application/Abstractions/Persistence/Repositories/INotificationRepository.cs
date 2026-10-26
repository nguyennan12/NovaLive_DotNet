using NovaLive.Domain.Notifications;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface INotificationRepository
{
    Task<bool> ExistsForReferenceAsync(Guid userId, string refType, Guid refId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
