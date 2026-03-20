using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.GetMyEvents;

public sealed class GetMyEventsHandler(ApplicationDbContext db)
    : IQueryHandler<GetMyEventsQuery, IEnumerable<EventDto>>
{
    public async Task<Result<IEnumerable<EventDto>>> HandleAsync(
        GetMyEventsQuery query,
        CancellationToken ct = default)
    {
        var events = await db.Events
            .AsNoTracking()
            .Where(e => e.OwnerId == query.UserId)
            .OrderBy(e => e.Date)
            .Select(e => new EventDto(
                e.Id,
                e.Title,
                e.Description,
                e.Date,
                e.GoogleCalendarEventId != null,
                e.LinkedWishlistId,
                e.CreatedAt))
            .ToListAsync(ct);

        return Result.Success<IEnumerable<EventDto>>(events);
    }
}
