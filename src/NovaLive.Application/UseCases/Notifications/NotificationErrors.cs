using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Notifications;

public static class NotificationErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized(
        "Notifications.Unauthorized",
        "Đăng nhập để xem thông báo.");

    public static readonly Error NotFound = Error.NotFound(
        "Notifications.NotFound",
        "Không tìm thấy thông báo.");
}
