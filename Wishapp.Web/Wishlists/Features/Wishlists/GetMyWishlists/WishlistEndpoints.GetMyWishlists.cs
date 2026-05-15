using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Features.Wishlists.GetMyWishlists;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<WishlistSummaryDto>>, UnauthorizedHttpResult>> GetMyWishlists(
        [AsParameters] PagedRequest request,
        WishlistSortBy sortBy,
        SortDirection direction,
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<GetMyWishlistsQuery, PagedResponse<WishlistSummaryDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new GetMyWishlistsQuery(userIdResult.Value, request, sortBy, direction), ct);

        return TypedResults.Ok(result.Value);
    }
}
