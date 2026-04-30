using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Features.Wishlists.GetWishlistQr;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<FileContentHttpResult, NotFound<Error>, BadRequest<Error>>> GetWishlistQr(
        [FromRoute] Guid id,
        HttpContext httpContext,
        IQueryHandler<GetWishlistQrQuery, byte[]> handler,
        CancellationToken ct)
    {
        var origin = httpContext.Request.Headers.Origin.ToString();
        var result = await handler.HandleAsync(new GetWishlistQrQuery(id, origin), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.File(result.Value, "image/png", $"wishlist-{id}-qr.png");
    }
}