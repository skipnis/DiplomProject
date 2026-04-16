using System.Text.Json;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications;

public sealed class NotificationsApi(ApplicationDbContext db) : INotificationsApi
{
    public async Task EnqueueAsync(Guid userId, NotificationType type, object payload, CancellationToken ct = default)
    {
        var element = JsonSerializer.SerializeToElement(payload);
        var notification = Notification.Create(userId, type, element);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }
}
