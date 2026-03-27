using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Users.Features.GetUserProfile;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<GetUserProfileResponse>, NotFound<Error>>> GetUserProfile(
        [FromRoute] Guid id,
        IQueryHandler<GetUserProfileQuery, GetUserProfileResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetUserProfileQuery(id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
