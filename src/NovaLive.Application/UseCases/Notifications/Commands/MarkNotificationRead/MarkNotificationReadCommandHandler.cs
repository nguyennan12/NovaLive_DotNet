using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task<Result> Handle(MarkNotificationReadCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty)
        {
            return NotificationErrors.Unauthorized;
        }

        var notification = await db.Notifications.FirstOrDefaultAsync(
            item => item.Id == command.NotificationId && item.UserId == userId.Value,
            ct);
        if (notification is null)
        {
            return NotificationErrors.NotFound;
        }

        notification.MarkAsRead(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
