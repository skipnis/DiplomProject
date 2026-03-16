using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishes.DeleteWish;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> DeleteWish(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        ICommandHandler<DeleteWishCommand> handler,
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

        var result = await handler.HandleAsync(new DeleteWishCommand(id, wishId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
