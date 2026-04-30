using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Features.Wishes.GetWishQr;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<FileContentHttpResult, NotFound<Error>, BadRequest<Error>>> GetWishQr(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        HttpContext httpContext,
        IQueryHandler<GetWishQrQuery, byte[]> handler,
        CancellationToken ct)
    {
        var origin = httpContext.Request.Headers.Origin.ToString();
        var result = await handler.HandleAsync(new GetWishQrQuery(id, wishId, origin), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.File(result.Value, "image/png", $"wish-{wishId}-qr.png");
    }
}