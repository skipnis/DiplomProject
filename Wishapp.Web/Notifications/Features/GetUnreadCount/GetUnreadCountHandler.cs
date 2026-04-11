using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Notifications.Features.GetUnreadCount;

public sealed class GetUnreadCountHandler(ApplicationDbContext db)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async Task<Result<int>> HandleAsync(GetUnreadCountQuery query, CancellationToken ct = default)
    {
        var count = await db.Notifications
            .CountAsync(n => n.UserId == query.UserId && !n.IsRead, ct);

        return count;
    }
}
