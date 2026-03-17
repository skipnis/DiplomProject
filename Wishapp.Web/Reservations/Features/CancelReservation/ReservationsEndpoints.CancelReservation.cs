using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> CancelReservation(
        [FromRoute] Guid wishId,
        ClaimsPrincipal user,
        ICommandHandler<Features.CancelReservation.CancelReservationCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new Features.CancelReservation.CancelReservationCommand(wishId, userIdResult.Value), ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.Forbid()
            };
        }

        return TypedResults.NoContent();
    }
}
