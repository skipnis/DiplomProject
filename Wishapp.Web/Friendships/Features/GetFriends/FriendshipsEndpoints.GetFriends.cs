using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Features.GetFriends;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Friendships;

public static partial class FriendshipsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<FriendInfo>>, UnauthorizedHttpResult>> GetFriends(
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        IQueryHandler<GetFriendsQuery, PagedResponse<FriendInfo>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new GetFriendsQuery(userIdResult.Value, request), ct);

        return TypedResults.Ok(result.Value);
    }
}
