using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Notifications.Features.DeleteNotification;

public sealed class DeleteNotificationHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteNotificationCommand>
{
    public async Task<Result> HandleAsync(DeleteNotificationCommand command, CancellationToken ct = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId, ct);

        if (notification is null)
            return Error.NotFound("Notifications.NotFound", "Notification not found");

        if (notification.UserId != command.UserId)
            return Error.Forbidden("Notifications.Forbidden", "Access denied");

        db.Notifications.Remove(notification);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
