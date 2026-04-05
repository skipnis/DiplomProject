using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Features.SyncAllEvents;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
    private static async Task<Results<NoContent, BadRequest<Error>, UnauthorizedHttpResult>> SyncAllEvents(
        ClaimsPrincipal user,
        ICommandHandler<SyncAllEventsCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new SyncAllEventsCommand(userIdResult.Value), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Error);
    }
}
