using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Users.Features.RefreshToken;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult>> RefreshToken(
        ICommandHandler<RefreshTokenCommand, RefreshTokenResponse> handler,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!httpContext.Request.Cookies.TryGetValue(CookieNames.RefreshToken, out var refreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var rememberMe = httpContext.Request.Cookies.ContainsKey(CookieNames.RememberMe);
        SetAuthCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken, rememberMe);

        return TypedResults.Ok();
    }
}
