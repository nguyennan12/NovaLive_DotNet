using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Notifications;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public Task<bool> ExistsForReferenceAsync(Guid userId, string refType, Guid refId, CancellationToken ct = default) =>
        db.Notifications.AsNoTracking().AnyAsync(n => n.UserId == userId && n.RefType == refType && n.RefId == refId, ct);

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default) =>
        await db.Notifications.AddRangeAsync(notifications, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
