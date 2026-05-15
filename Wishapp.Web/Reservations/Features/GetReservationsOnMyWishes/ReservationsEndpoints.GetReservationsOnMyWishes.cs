using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Reservations.Dtos;

namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    private static async Task<Results<Ok<List<WishReservedOnMyWishDto>>, UnauthorizedHttpResult>> GetReservationsOnMyWishes(
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<Features.GetReservationsOnMyWishes.GetReservationsOnMyWishesQuery, List<WishReservedOnMyWishDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new Features.GetReservationsOnMyWishes.GetReservationsOnMyWishesQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }
}
