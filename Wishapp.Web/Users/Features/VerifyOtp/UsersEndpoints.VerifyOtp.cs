using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Users.Features.VerifyOtp;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult>> VerifyOtp(
        VerifyOtpCommand command,
        ICommandHandler<VerifyOtpCommand, VerifyOtpResponse> handler,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure) return TypedResults.Unauthorized();

        SetAuthCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken);

        return TypedResults.Ok();
    }
}
