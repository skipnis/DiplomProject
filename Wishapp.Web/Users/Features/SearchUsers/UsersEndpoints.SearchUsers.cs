using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.SearchUsers;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<UsersSearchResponse>, UnauthorizedHttpResult>> SearchUsers(
        [FromQuery] string username,
        ClaimsPrincipal user,
        IQueryHandler<SearchUsersQuery, UsersSearchResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new SearchUsersQuery(username, userIdResult.Value), ct);
        return TypedResults.Ok(result.Value);
    }
}
