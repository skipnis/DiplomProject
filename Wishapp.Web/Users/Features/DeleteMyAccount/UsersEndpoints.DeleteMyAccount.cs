using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.DeleteMyAccount;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, UnauthorizedHttpResult, BadRequest<string>>> DeleteMyAccount(
        [FromBody] DeleteMyAccountRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICommandHandler<DeleteMyAccountCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new DeleteMyAccountCommand(userIdResult.Value, request.Code), ct);

        if (result.IsFailure)
        {
            if (result.Error.Type == ErrorType.Unauthorized)
                return TypedResults.Unauthorized();

            return TypedResults.BadRequest(result.Error.Description);
        }

        httpContext.Response.Cookies.Delete("access_token");
        httpContext.Response.Cookies.Delete("refresh_token");

        return TypedResults.NoContent();
    }
}

public record DeleteMyAccountRequest(string Code);
