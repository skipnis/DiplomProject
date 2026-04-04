using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Features.Wishes.RegenerateWishShareToken;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<RegenerateWishShareTokenResponse>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> RegenerateWishShareToken(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        ICommandHandler<RegenerateWishShareTokenCommand, Guid> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null || accessContext.OwnerId != userIdResult.Value)
            return TypedResults.Forbid();

        var result = await handler.HandleAsync(new RegenerateWishShareTokenCommand(id, wishId), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(new RegenerateWishShareTokenResponse(result.Value));
    }
}

public record RegenerateWishShareTokenResponse(Guid Token);
