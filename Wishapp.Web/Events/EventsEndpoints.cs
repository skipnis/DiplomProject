namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/events").RequireAuthorization();

        events.MapPost("/", CreateEvent).Produces(401);
        events.MapGet("/my", GetMyEvents).Produces(401);
        events.MapGet("/{id:guid}", GetEvent).Produces(401);
        events.MapPut("/{id:guid}", UpdateEvent).Produces(401);
        events.MapDelete("/{id:guid}", DeleteEvent).Produces(401);
        events.MapPut("/{id:guid}/wishlist", LinkWishlist).Produces(401);
        events.MapPost("/google-calendar/sync-all", SyncAllEvents).Produces(401);
        events.MapPost("/{id:guid}/google-calendar/sync", SyncToGoogleCalendar).Produces(401);
        events.MapDelete("/{id:guid}/google-calendar/sync", UnsyncFromGoogleCalendar).Produces(401);

        return app;
    }
}
