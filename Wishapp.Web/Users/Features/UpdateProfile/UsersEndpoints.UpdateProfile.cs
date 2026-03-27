using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.UpdateProfile;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> UpdateProfile(
        UpdateProfileRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UpdateProfileCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new UpdateProfileCommand(userIdResult.Value, request.Username, request.Bio, request.BirthDate), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
