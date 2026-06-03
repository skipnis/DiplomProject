using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Dtos;
using Wishapp.Web.Events.Features.GetEventByWishlist;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Events;

public static partial class EventsEndpoints
{
    private static async Task<Results<Ok<EventDto>, NotFound<Error>, UnauthorizedHttpResult>> GetEventByWishlist(
        Guid wishlistId,
        ClaimsPrincipal user,
        IQueryHandler<GetEventByWishlistQuery, EventDto> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new GetEventByWishlistQuery(wishlistId, userIdResult.Value), ct);

        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        return TypedResults.NotFound(result.Error);
    }
}
