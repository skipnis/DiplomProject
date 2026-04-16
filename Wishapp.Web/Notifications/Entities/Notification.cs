using System.Text.Json;

namespace Wishapp.Web.Notifications.Entities;

public sealed class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public JsonElement Payload { get; private set; }
    public bool IsRead { get; private set; }
    public NotificationStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private Notification() { }

    public static Notification Create(Guid userId, NotificationType type, JsonElement payload)
    {
        return new Notification
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Type = type,
            Payload = payload,
            IsRead = false,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void RecordFailure(int maxRetries)
    {
        RetryCount++;
        if (RetryCount >= maxRetries)
        {
            Status = NotificationStatus.Failed;
            ProcessedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkRead()
    {
        IsRead = true;
    }
}
