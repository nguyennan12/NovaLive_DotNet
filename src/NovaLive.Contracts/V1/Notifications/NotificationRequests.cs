namespace NovaLive.Contracts.V1.Notifications;

public sealed record GetNotificationsRequest(
    int Page = 1,
    int Size = 20,
    bool? IsRead = null);
