using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<List<BlacklistItemResponse>>, UnauthorizedHttpResult>> GetMyBlacklist(
        ClaimsPrincipal user,
        IQueryHandler<GetMyBlacklistQuery, List<BlacklistItemResponse>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new GetMyBlacklistQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }
}
