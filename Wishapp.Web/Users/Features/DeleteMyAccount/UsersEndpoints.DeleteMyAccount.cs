using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.DeleteMyAccount;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeleteMyAccount(
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICommandHandler<DeleteMyAccountCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        await handler.HandleAsync(new DeleteMyAccountCommand(userIdResult.Value), ct);

        httpContext.Response.Cookies.Delete("access_token");
        httpContext.Response.Cookies.Delete("refresh_token");

        return TypedResults.NoContent();
    }
}
