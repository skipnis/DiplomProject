using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Features.Wishlists.GetUserWishlists;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Ok<PagedResponse<WishlistSummaryDto>>> GetUserWishlists(
        [FromRoute] Guid userId,
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<GetUserWishlistsQuery, PagedResponse<WishlistSummaryDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        var currentUserId = userIdResult.IsSuccess ? userIdResult.Value : (Guid?)null;

        var result = await handler.HandleAsync(
            new GetUserWishlistsQuery(currentUserId, userId, request), ct);

        return TypedResults.Ok(result.Value);
    }
}