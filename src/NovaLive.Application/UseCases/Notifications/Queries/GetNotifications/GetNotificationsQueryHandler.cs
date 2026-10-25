using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Notifications;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponse>>
{
    public async Task<Result<PagedResult<NotificationResponse>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty)
        {
            return NotificationErrors.Unauthorized;
        }

        var request = query.Request;
        var page = Math.Max(1, request.Page);
        var size = request.Size <= 0 ? 20 : Math.Min(request.Size, 100);
        var notifications = db.Notifications.AsNoTracking().Where(item => item.UserId == userId.Value);
        if (request.IsRead.HasValue)
        {
            notifications = notifications.Where(item => item.IsRead == request.IsRead.Value);
        }

        var total = await notifications.LongCountAsync(ct);
        var items = await notifications
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(item => new
            {
                item.Id,
                item.Type,
                item.Title,
                item.Body,
                item.IsRead,
                item.RefType,
                item.RefId,
                item.CreatedAt
            })
            .ToListAsync(ct);

        var response = items.Select(item => new NotificationResponse(
            item.Id,
            item.Type,
            item.Title,
            item.Body,
            item.IsRead,
            item.RefId.HasValue && item.RefType == "SubOrder"
                ? $"/seller/orders/{item.RefId.Value}"
                : "/seller/notifications",
            item.CreatedAt)).ToList();

        return PagedResult<NotificationResponse>.Create(response, page, size, total);
    }
}
