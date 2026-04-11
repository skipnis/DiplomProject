using System.Text.Json;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications;

public sealed class NotificationsApi(ApplicationDbContext db) : INotificationsApi
{
    public async Task EnqueueAsync(Guid userId, NotificationType type, object payload, CancellationToken ct = default)
    {
        var document = JsonSerializer.SerializeToDocument(payload);
        var notification = Notification.Create(userId, type, document);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }
}
