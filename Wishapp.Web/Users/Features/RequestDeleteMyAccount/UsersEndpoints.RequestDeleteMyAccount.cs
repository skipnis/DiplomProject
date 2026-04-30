using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.RequestDeleteMyAccount;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult, BadRequest<string>>> RequestAccountDeletion(
        ClaimsPrincipal user,
        ICommandHandler<RequestDeleteMyAccountCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new RequestDeleteMyAccountCommand(userIdResult.Value), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error.Description);

        return TypedResults.Ok();
    }
}
