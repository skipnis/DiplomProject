using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Events.Features.GetEvent;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
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
}
