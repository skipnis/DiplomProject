using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications;

public interface INotificationsApi
{
    Task EnqueueAsync(Guid userId, NotificationType type, object payload, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
