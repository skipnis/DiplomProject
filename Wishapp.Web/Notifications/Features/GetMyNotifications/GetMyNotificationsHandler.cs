using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications.Dtos;

namespace Wishapp.Web.Notifications.Features.GetMyNotifications;

public sealed class GetMyNotificationsHandler(ApplicationDbContext db)
    : IQueryHandler<GetMyNotificationsQuery, PagedResponse<NotificationDto>>
{
    public async Task<Result<PagedResponse<NotificationDto>>> HandleAsync(
        GetMyNotificationsQuery query,
        CancellationToken ct = default)
    {
        var notifications = db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId);

        if (query.From.HasValue)
        {
            var fromUtc = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            notifications = notifications.Where(n => n.CreatedAt >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toUtc = query.To.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            notifications = notifications.Where(n => n.CreatedAt <= toUtc);
        }

        if (query.IsRead.HasValue)
            notifications = notifications.Where(n => n.IsRead == query.IsRead.Value);

        var result = await notifications
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Payload,
                n.IsRead,
                n.CreatedAt))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
