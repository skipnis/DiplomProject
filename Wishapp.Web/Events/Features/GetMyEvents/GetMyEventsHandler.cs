using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.GetMyEvents;

public sealed class GetMyEventsHandler(ApplicationDbContext db)
    : IQueryHandler<GetMyEventsQuery, PagedResponse<EventDto>>
{
    public async Task<Result<PagedResponse<EventDto>>> HandleAsync(
        GetMyEventsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.Events
            .AsNoTracking()
            .Where(e => e.OwnerId == query.UserId)
            .OrderBy(e => e.Date)
            .Select(e => new EventDto(
                e.Id,
                e.Title,
                e.Description,
                e.Date,
                e.LinkedWishlistId,
                e.CreatedAt))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
