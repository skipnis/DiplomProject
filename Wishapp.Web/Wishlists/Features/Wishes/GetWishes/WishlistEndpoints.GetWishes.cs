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
    private static async Task<Results<Ok<PagedResponse<WishSummaryDto>>, ForbidHttpResult, UnauthorizedHttpResult>> GetWishes(
        [FromRoute] Guid id,
        [AsParameters] PagedRequest request,
        WishSortBy sortBy,
        SortDirection direction,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        IFriendshipsApi friendshipsApi,
        [FromServices] IQueryHandler<GetWishesQuery, PagedResponse<WishSummaryDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        var userId = userIdResult.IsSuccess ? userIdResult.Value : (Guid?)null;

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null)
        {
            return TypedResults.Forbid();
        }

        var isOwner = userId.HasValue && accessContext.OwnerId == userId.Value;

        var authorized = isOwner || accessContext.Visibility switch
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
            WishlistVisibility.Private => false,
            _ => false
        };

        if (!authorized)
        {
            return TypedResults.Forbid();
        }

        var result = await handler.HandleAsync(new GetWishesQuery(id, request, accessContext.IsSurpriseModeEnabled, sortBy, direction), ct);

        return TypedResults.Ok(result.Value);
    }
}
