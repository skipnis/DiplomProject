using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Users.Features.RefreshToken;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult>> RefreshToken(
        ICommandHandler<RefreshTokenCommand, RefreshTokenResponse> handler,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!httpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        SetAuthCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken);

        return TypedResults.Ok();
    }
}
