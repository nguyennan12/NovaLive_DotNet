namespace NovaLive.Contracts.V1.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Content,
    bool IsRead,
    string? Link,
    DateTimeOffset CreatedAt);
