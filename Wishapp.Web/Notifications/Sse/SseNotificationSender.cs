using Wishapp.Web.Notifications.Dtos;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications.Sse;

public sealed class SseNotificationSender(SseConnectionManager connections) : INotificationSender
{
    public Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        var dto = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Payload.Clone(),
            notification.IsRead,
            notification.CreatedAt);

        foreach (var channel in connections.GetChannels(notification.UserId))
            channel.Writer.TryWrite(dto);

        return Task.CompletedTask;
    }
}
