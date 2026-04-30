using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<List<Features.Wishes.GetMyFulfilledWishes.FulfilledWishRecordDto>>, UnauthorizedHttpResult>> GetMyFulfilledWishes(
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<Features.Wishes.GetMyFulfilledWishes.GetMyFulfilledWishesQuery, List<Features.Wishes.GetMyFulfilledWishes.FulfilledWishRecordDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.Wishes.GetMyFulfilledWishes.GetMyFulfilledWishesQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }
}
