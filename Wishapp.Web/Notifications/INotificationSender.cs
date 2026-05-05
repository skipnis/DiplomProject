using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications;

public interface INotificationSender
{
    Task SendAsync(Notification notification, CancellationToken ct = default);
}
