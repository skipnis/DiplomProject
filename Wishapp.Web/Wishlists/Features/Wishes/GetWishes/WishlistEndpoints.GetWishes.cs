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
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishes.GetWishes;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<WishDto>>, ForbidHttpResult, UnauthorizedHttpResult>> GetWishes(
        [FromRoute] Guid id,
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        IFriendshipsApi friendshipsApi,
        [FromServices] IQueryHandler<GetWishesQuery, PagedResponse<WishDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        var userId = userIdResult.IsSuccess ? userIdResult.Value : (Guid?)null;

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null)
        {
            return TypedResults.Forbid();
        }

        var authorized = accessContext.Visibility switch
        {
            WishlistVisibility.Public => true,
            WishlistVisibility.Friends when userId.HasValue => (await authorizationService
                .AuthorizeAsync(user, accessContext with
                {
                    AreFriends = await friendshipsApi.AreFriendsAsync(userId.Value, accessContext.OwnerId, ct)
                }, new WishlistFriendRequirement())).Succeeded,
            WishlistVisibility.SelectedFriends when userId.HasValue => (await authorizationService
                    .AuthorizeAsync(user, accessContext, new WishlistMemberRequirement(WishlistMemberRole.Viewer)))
                .Succeeded,
            WishlistVisibility.Private when userId.HasValue => accessContext.OwnerId == userId.Value,
            _ => false
        };

        if (!authorized)
        {
            return TypedResults.Forbid();
        }

        var result = await handler.HandleAsync(new GetWishesQuery(id, request), ct);

        return TypedResults.Ok(result.Value);
    }
}
