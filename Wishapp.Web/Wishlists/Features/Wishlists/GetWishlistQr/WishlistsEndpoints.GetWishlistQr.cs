using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.QrCode;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishlists.GetWishlistQr;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<FileContentHttpResult, NotFound<Error>, BadRequest<Error>>> GetWishlistQr(
        [FromRoute] Guid id,
        IQueryHandler<GetWishlistQrQuery, byte[]> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetWishlistQrQuery(id), ct);

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