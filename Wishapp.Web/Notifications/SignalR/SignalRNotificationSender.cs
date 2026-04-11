using Microsoft.AspNetCore.SignalR;
using Wishapp.Web.Notifications.Dtos;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Notifications.Interfaces;

namespace Wishapp.Web.Notifications.SignalR;

public sealed class SignalRNotificationSender(IHubContext<NotificationsHub> hubContext) : INotificationSender
{
    public async Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        var dto = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Payload.RootElement.Clone(),
            notification.IsRead,
            notification.CreatedAt);

        await hubContext.Clients
            .User(notification.UserId.ToString())
            .SendAsync("ReceiveNotification", dto, ct);
    }
}
