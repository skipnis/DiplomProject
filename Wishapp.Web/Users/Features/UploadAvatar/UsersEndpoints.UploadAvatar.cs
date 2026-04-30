using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.UploadAvatar;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<UploadAvatarResponse>, BadRequest<Error>, NotFound<Error>, UnauthorizedHttpResult>> UploadAvatar(
        [FromForm] IFormFile file,
        ClaimsPrincipal user,
        ICommandHandler<UploadAvatarCommand, UploadAvatarResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new UploadAvatarCommand(userIdResult.Value, file), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
