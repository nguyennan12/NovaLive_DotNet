using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Notifications.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<MarkAllNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty)
        {
            return NotificationErrors.Unauthorized;
        }

        var unread = await db.Notifications
            .Where(item => item.UserId == userId.Value && !item.IsRead)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var notification in unread)
        {
            notification.MarkAsRead(now);
        }

        if (unread.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
