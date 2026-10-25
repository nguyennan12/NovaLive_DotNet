using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Notifications;

namespace NovaLive.Application.UseCases.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(GetNotificationsRequest Request)
    : IQuery<Contracts.Common.PagedResult<NotificationResponse>>;
