using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Features.GetFriendshipRequests;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Friendships;

public static partial class FriendshipsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<FriendshipRequest>>, UnauthorizedHttpResult>> GetFriendshipRequests(
        [AsParameters] GetFriendshipRequestsRequest request,
        ClaimsPrincipal user,
        IQueryHandler<GetFriendshipRequestsQuery, PagedResponse<FriendshipRequest>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new GetFriendshipRequestsQuery(
                userIdResult.Value,
                request.Status,
                new PagedRequest(request.Page, request.PageSize)), ct);

        return TypedResults.Ok(result.Value);
    }
}
