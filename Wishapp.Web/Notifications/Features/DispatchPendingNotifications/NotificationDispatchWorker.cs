using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications.Features.DispatchPendingNotifications;

public sealed class NotificationDispatchWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDispatchWorker> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DispatchBatchAsync(stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        var batch = await db.Notifications
            .Where(n => n.Status == NotificationStatus.Pending && n.RetryCount < MaxRetries)
            .OrderBy(n => n.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        foreach (var notification in batch)
        {
            try
            {
                await sender.SendAsync(notification, ct);
                notification.MarkSent();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send notification {Id}, attempt {Attempt}",
                    notification.Id, notification.RetryCount + 1);
                notification.RecordFailure(MaxRetries);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
