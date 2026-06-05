using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<Features.Wishes.FulfillWish.FulfillWishResult>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> FulfillWish(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        ICommandHandler<Features.Wishes.FulfillWish.FulfillWishCommand, Features.Wishes.FulfillWish.FulfillWishResult> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null)
        {
            return TypedResults.Forbid();
        }

        var authorized = (await authorizationService
                .AuthorizeAsync(user, accessContext, new WishlistMemberRequirement(WishlistMemberRole.Editor)))
            .Succeeded;

        if (!authorized)
        {
            return TypedResults.Forbid();
        }

        var wishCreatorId = await db.Wishes
            .AsNoTracking()
            .Where(w => w.Id == wishId && w.WishlistId == id)
            .Select(w => w.CreatedByUserId)
            .FirstOrDefaultAsync(ct);

        var isWishAuthor = wishCreatorId == userIdResult.Value
            || (wishCreatorId == null && accessContext.OwnerId == userIdResult.Value);

        if (!isWishAuthor)
            return TypedResults.Forbid();

        var result = await handler.HandleAsync(new Features.Wishes.FulfillWish.FulfillWishCommand(id, wishId, userIdResult.Value), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
