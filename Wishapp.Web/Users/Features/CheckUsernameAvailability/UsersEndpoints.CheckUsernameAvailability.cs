using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.CheckUsernameAvailability;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, Conflict<Error>, UnauthorizedHttpResult>> CheckUsernameAvailability(
        string username,
        ClaimsPrincipal user,
        IQueryHandler<CheckUsernameAvailabilityQuery, bool> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new CheckUsernameAvailabilityQuery(userIdResult.Value, username), ct);

        return result.Value
            ? TypedResults.Ok()
            : TypedResults.Conflict(Error.Conflict("Users.UsernameAlreadyTaken", "Username is already taken"));
    }
}
