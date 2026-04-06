using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Features.Wishes.GetSharedWish;

namespace Wishapp.Web.Wishlists;

public static class ShareEndpoints
{
    public static IEndpointRouteBuilder MapShareEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/share/{token:guid}", GetSharedWish).AllowAnonymous();

        return app;
    }

    private static async Task<Results<Ok<SharedWishResponse>, NotFound<Error>>> GetSharedWish(
        Guid token,
        IQueryHandler<GetSharedWishQuery, SharedWishResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetSharedWishQuery(token), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
