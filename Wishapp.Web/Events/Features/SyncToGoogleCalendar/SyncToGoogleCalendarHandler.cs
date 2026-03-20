using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.GoogleCalendar;
using Wishapp.Web.Users;

namespace Wishapp.Web.Events.Features.SyncToGoogleCalendar;

public sealed class SyncToGoogleCalendarHandler(
    ApplicationDbContext db,
    IGoogleCalendarService calendarService,
    IUsersApi usersApi)
    : ICommandHandler<SyncToGoogleCalendarCommand>
{
    public async Task<Result> HandleAsync(
        SyncToGoogleCalendarCommand command,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
            return Error.NotFound("Events.NotFound", "Event not found");

        if (@event.OwnerId != command.UserId)
            return Error.Forbidden("Events.Forbidden", "Access denied");

        var refreshTokenResult = await usersApi.GetExternalRefreshTokenAsync(
            command.UserId, "google", "calendar", ct);

        if (refreshTokenResult.IsFailure)
            return Error.Failure("GoogleCalendar.NotConnected", "Google Calendar is not connected. Connect it in your profile settings.");

        var accessTokenResult = await calendarService.GetAccessTokenAsync(refreshTokenResult.Value, ct);

        if (accessTokenResult.IsFailure)
            return accessTokenResult.Error;

        var data = new GoogleCalendarEventData(@event.Title, @event.Description, @event.Date);

        if (@event.GoogleCalendarEventId is not null)
        {
            var updateResult = await calendarService.UpdateEventAsync(
                accessTokenResult.Value,
                @event.GoogleCalendarEventId,
                data,
                ct);

            if (updateResult.IsFailure)
                return updateResult;
        }
        else
        {
            var createResult = await calendarService.CreateEventAsync(
                accessTokenResult.Value,
                data,
                ct);

            if (createResult.IsFailure)
                return createResult;

            @event.GoogleCalendarEventId = createResult.Value;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
