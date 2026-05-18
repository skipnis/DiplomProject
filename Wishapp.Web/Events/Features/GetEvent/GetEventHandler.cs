using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.GetEvent;

public sealed class GetEventHandler(ApplicationDbContext db)
    : IQueryHandler<GetEventQuery, EventDto>
{
    public async Task<Result<EventDto>> HandleAsync(
        GetEventQuery query,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.EventId, ct);

        if (@event is null)
        {
            return Error.NotFound("Events.NotFound", "Event not found");
        }

        if (@event.OwnerId != query.UserId)
        {
            return Error.Forbidden("Events.Forbidden", "Access denied");
        }

        return new EventDto(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Date,
            @event.LinkedWishlistId,
            @event.CreatedAt);
    }
}
