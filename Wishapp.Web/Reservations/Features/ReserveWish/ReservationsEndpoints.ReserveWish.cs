using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    private static async Task<Results<Created, NotFound<Error>, Conflict<Error>, ForbidHttpResult, UnauthorizedHttpResult>> ReserveWish(
        [FromRoute] Guid wishId,
        [FromBody] Features.ReserveWish.ReserveWishRequest request,
        ClaimsPrincipal user,
        ICommandHandler<Features.ReserveWish.ReserveWishCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var command = new Features.ReserveWish.ReserveWishCommand(wishId, request.WishlistId, userIdResult.Value);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                ErrorType.Conflict => TypedResults.Conflict(result.Error),
                _ => TypedResults.Forbid()
            };
        }

        return TypedResults.Created();
    }
}
