using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Events.Features.CreateEvent;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
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
}
