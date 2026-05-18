using Wishapp.Web.Events.Features.CreateEvent;
using Wishapp.Web.Events.Features.LinkWishlist;
using Wishapp.Web.Events.Features.UpdateEvent;
using Wishapp.Web.Infrastructure.Validation;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/events").RequireAuthorization();

        events.MapPost("/", CreateEvent).Produces(401)
            .AddEndpointFilter<ValidationFilter<CreateEventRequest>>();
        events.MapGet("/my", GetMyEvents).Produces(401);
        events.MapGet("/{id:guid}", GetEvent).Produces(401);
        events.MapPut("/{id:guid}", UpdateEvent).Produces(401)
            .AddEndpointFilter<ValidationFilter<UpdateEventRequest>>();
        events.MapDelete("/{id:guid}", DeleteEvent).Produces(401);
        events.MapPut("/{id:guid}/wishlist", LinkWishlist).Produces(401)
            .AddEndpointFilter<ValidationFilter<LinkWishlistRequest>>();
        return app;
    }
}
