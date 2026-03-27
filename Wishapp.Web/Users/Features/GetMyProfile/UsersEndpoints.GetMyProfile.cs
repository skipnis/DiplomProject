using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.GetMyProfile;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<GetMyProfileResponse>, UnauthorizedHttpResult>> GetMyProfile(
        ClaimsPrincipal user,
        IQueryHandler<GetMyProfileQuery, GetMyProfileResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new GetMyProfileQuery(userIdResult.Value), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }
}
