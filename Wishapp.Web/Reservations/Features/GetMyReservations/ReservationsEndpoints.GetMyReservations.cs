using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Reservations.Dtos;

namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<MyReservationDto>>, UnauthorizedHttpResult>> GetMyReservations(
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<Features.GetMyReservations.GetMyReservationsQuery, PagedResponse<MyReservationDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new Features.GetMyReservations.GetMyReservationsQuery(userIdResult.Value, request), ct);

        return TypedResults.Ok(result.Value);
    }
}
