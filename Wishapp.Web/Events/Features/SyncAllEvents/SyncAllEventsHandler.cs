using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.GoogleCalendar;
using Wishapp.Web.Users;

namespace Wishapp.Web.Events.Features.SyncAllEvents;

public sealed class SyncAllEventsHandler(
    ApplicationDbContext db,
    IGoogleCalendarService calendarService,
    IUsersApi usersApi)
    : ICommandHandler<SyncAllEventsCommand>
{
    public async Task<Result> HandleAsync(SyncAllEventsCommand command, CancellationToken ct = default)
    {
        var refreshTokenResult = await usersApi.GetExternalRefreshTokenAsync(
            command.UserId, "google", "calendar", ct);

        if (refreshTokenResult.IsFailure)
            return refreshTokenResult.Error;

        var accessTokenResult = await calendarService.GetAccessTokenAsync(refreshTokenResult.Value, ct);

        if (accessTokenResult.IsFailure)
            return accessTokenResult.Error;

        var accessToken = accessTokenResult.Value;

        var events = await db.Events
            .Where(e => e.OwnerId == command.UserId)
            .ToListAsync(ct);

        foreach (var @event in events)
        {
            var data = new GoogleCalendarEventData(@event.Title, @event.Description, @event.Date);

            if (@event.GoogleCalendarEventId is not null)
            {
                await calendarService.UpdateEventAsync(accessToken, @event.GoogleCalendarEventId, data, ct);
            }
            else
            {
                var createResult = await calendarService.CreateEventAsync(accessToken, data, ct);
                if (createResult.IsSuccess)
                    @event.SetGoogleCalendarEventId(createResult.Value);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
