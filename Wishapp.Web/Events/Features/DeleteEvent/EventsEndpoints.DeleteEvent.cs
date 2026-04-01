using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Features.DeleteEvent;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
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
}
