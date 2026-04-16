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
        var result = await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId)
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
