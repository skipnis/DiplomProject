using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Events.Features.CreateEvent;
using Wishapp.Web.Events.Features.DeleteEvent;
using Wishapp.Web.Events.Features.GetEvent;
using Wishapp.Web.Events.Features.GetMyEvents;
using Wishapp.Web.Events.Features.LinkWishlist;
using Wishapp.Web.Events.Features.SyncAllEvents;
using Wishapp.Web.Events.Features.SyncToGoogleCalendar;
using Wishapp.Web.Events.Features.UnsyncFromGoogleCalendar;
using Wishapp.Web.Events.Features.UpdateEvent;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static class EventsEndpoints
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

    private static async Task<Results<Created<CreateEventResponse>, UnauthorizedHttpResult>> CreateEvent(
        [FromBody] CreateEventRequest request,
        ClaimsPrincipal user,
        ICommandHandler<CreateEventCommand, CreateEventResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new CreateEventCommand(userIdResult.Value, request.Title, request.Description, request.Date), ct);

        return TypedResults.Created($"/events/{result.Value.Id}", result.Value);
    }

    private static async Task<Results<Ok<IEnumerable<EventDto>>, UnauthorizedHttpResult>> GetMyEvents(
        ClaimsPrincipal user,
        IQueryHandler<GetMyEventsQuery, IEnumerable<EventDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new GetMyEventsQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<EventDto>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> GetEvent(
        Guid id,
        ClaimsPrincipal user,
        IQueryHandler<GetEventQuery, EventDto> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }
        
        var result = await handler.HandleAsync(new GetEventQuery(id, userIdResult.Value), ct);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.NotFound(result.Error)
        };
    }

    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UpdateEventCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }
        
        var result = await handler.HandleAsync(
            new UpdateEventCommand(id, userIdResult.Value, request.Title, request.Description, request.Date), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.NotFound(result.Error)
        };
    }

    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> DeleteEvent(
        Guid id,
        ClaimsPrincipal user,
        ICommandHandler<DeleteEventCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }
        
        var result = await handler.HandleAsync(new DeleteEventCommand(id, userIdResult.Value), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.NotFound(result.Error)
        };
    }

    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> LinkWishlist(
        Guid id,
        [FromBody] LinkWishlistRequest request,
        ClaimsPrincipal user,
        ICommandHandler<LinkWishlistCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }
        
        var result = await handler.HandleAsync(
            new LinkWishlistCommand(id, userIdResult.Value, request.WishlistId), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.NotFound(result.Error)
        };
    }

    private static async Task<Results<NoContent, BadRequest<Error>, UnauthorizedHttpResult>> SyncAllEvents(
        ClaimsPrincipal user,
        ICommandHandler<SyncAllEventsCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new SyncAllEventsCommand(userIdResult.Value), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, BadRequest<Error>, UnauthorizedHttpResult>> SyncToGoogleCalendar(
        Guid id,
        ClaimsPrincipal user,
        ICommandHandler<SyncToGoogleCalendarCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new SyncToGoogleCalendarCommand(id, userIdResult.Value), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.BadRequest(result.Error)
        };
    }

    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> UnsyncFromGoogleCalendar(
        Guid id,
        ClaimsPrincipal user,
        ICommandHandler<UnsyncFromGoogleCalendarCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new UnsyncFromGoogleCalendarCommand(id, userIdResult.Value), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.NotFound(result.Error)
        };
    }
}
