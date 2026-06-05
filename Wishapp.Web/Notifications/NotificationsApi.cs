using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications;

public sealed class NotificationsApi(ApplicationDbContext db, INotificationSender sender, ILogger<NotificationsApi> logger) : INotificationsApi
{
    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Notifications
            .Where(n => n.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task EnqueueAsync(Guid userId, NotificationType type, object payload, CancellationToken ct = default)
    {
        var element = JsonSerializer.SerializeToElement(payload);
        var notification = Notification.Create(userId, type, element);
        db.Notifications.Add(notification);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save notification {NotificationType} for user {UserId}", type, userId);
            throw;
        }

        try
        {
            await sender.SendAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send notification {NotificationType} to user {UserId} via SSE", type, userId);
        }
    }
}
