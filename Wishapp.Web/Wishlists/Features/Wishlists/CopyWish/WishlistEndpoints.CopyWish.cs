using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Authorization;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishlists.CopyWish;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async
        Task<Results<Created<CopyWishResponse>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> CopyWish(
            [FromRoute] Guid id,
            [FromRoute] Guid wishId,
            CopyWishRequest request,
            ClaimsPrincipal user,
            ApplicationDbContext db,
            IAuthorizationService authorizationService,
            IFriendshipsApi friendshipsApi,
            ICommandHandler<CopyWishCommand, CopyWishResponse> handler,
            CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var (sourceAccessContext, targetAccessContext) =
            await db.GetAccessContextsAsync(id, request.TargetWishlistId, ct);

        if (sourceAccessContext is null)
        {
            return TypedResults.Forbid();
        }

        var isSourceOwner = sourceAccessContext.OwnerId == userIdResult.Value;

        var canViewSource = isSourceOwner || sourceAccessContext.Visibility switch
        {
            WishlistVisibility.Public => true,
            WishlistVisibility.Friends => (await authorizationService
                .AuthorizeAsync(user, sourceAccessContext with
                {
                    AreFriends =
                    await friendshipsApi.AreFriendsAsync(userIdResult.Value, sourceAccessContext.OwnerId, ct)
                }, new WishlistFriendRequirement())).Succeeded,
            WishlistVisibility.SelectedFriends => (await authorizationService
                    .AuthorizeAsync(user, sourceAccessContext,
                        new WishlistMemberRequirement(WishlistMemberRole.Viewer)))
                .Succeeded,
            WishlistVisibility.Private => false,
            _ => false
        };

        if (!canViewSource || targetAccessContext is null)
        {
            return TypedResults.Forbid();
        }

        var canEditTarget = (await authorizationService
                .AuthorizeAsync(user, targetAccessContext, new WishlistMemberRequirement(WishlistMemberRole.Editor)))
            .Succeeded;

        if (!canEditTarget)
        {
            return TypedResults.Forbid();
        }

        var result = await handler.HandleAsync(
            new CopyWishCommand(id, wishId, request.TargetWishlistId, userIdResult.Value), ct);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.Created(
            $"/wishlists/{request.TargetWishlistId}/wishes/{result.Value.WishId}",
            result.Value);
    }
}
