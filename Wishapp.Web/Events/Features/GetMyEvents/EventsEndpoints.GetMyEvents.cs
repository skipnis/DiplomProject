using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Events.Features.GetMyEvents;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<EventDto>>, UnauthorizedHttpResult>> GetMyEvents(
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        IQueryHandler<GetMyEventsQuery, PagedResponse<EventDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new GetMyEventsQuery(userIdResult.Value, request), ct);

        return TypedResults.Ok(result.Value);
    }
}
