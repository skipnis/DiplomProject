using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Features.SyncToGoogleCalendar;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
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
}
