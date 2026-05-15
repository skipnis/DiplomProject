using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events;

public sealed class EventsApi(ApplicationDbContext db) : IEventsApi
{
    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Events
            .Where(e => e.OwnerId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
