using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications.Interfaces;

public interface INotificationSender
{
    Task SendAsync(Notification notification, CancellationToken ct = default);
}
