using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand;
