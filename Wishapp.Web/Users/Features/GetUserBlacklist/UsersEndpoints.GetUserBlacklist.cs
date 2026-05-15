using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.GetMyBlacklist;
using Wishapp.Web.Users.Features.GetUserBlacklist;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<List<BlacklistItemResponse>>, ForbidHttpResult, UnauthorizedHttpResult>> GetUserBlacklist(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        IQueryHandler<GetUserBlacklistQuery, List<BlacklistItemResponse>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new GetUserBlacklistQuery(userIdResult.Value, id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Forbid();
    }
}
