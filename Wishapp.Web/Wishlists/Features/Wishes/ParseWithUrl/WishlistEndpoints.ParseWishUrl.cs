using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Features.Wishes.ParseWithUrl;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<ParsedWishData>, BadRequest<Error>, UnauthorizedHttpResult>> ParseWishUrl(
        ParseWishUrlRequest request,
        ClaimsPrincipal user,
        IQueryHandler<ParseWishUrlQuery, ParsedWishData> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new ParseWishUrlQuery(request.Url), ct);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }
}
