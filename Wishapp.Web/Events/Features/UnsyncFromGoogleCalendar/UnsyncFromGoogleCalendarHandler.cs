using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.UnsyncFromGoogleCalendar;

public sealed class UnsyncFromGoogleCalendarHandler(ApplicationDbContext db)
    : ICommandHandler<UnsyncFromGoogleCalendarCommand>
{
    public async Task<Result> HandleAsync(
        UnsyncFromGoogleCalendarCommand command,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
        {
            return Error.NotFound("Events.NotFound", "Event not found");
        }

        if (@event.OwnerId != command.UserId)
        {
            return Error.Forbidden("Events.Forbidden", "Access denied");
        }

        @event.GoogleCalendarEventId = null;
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
