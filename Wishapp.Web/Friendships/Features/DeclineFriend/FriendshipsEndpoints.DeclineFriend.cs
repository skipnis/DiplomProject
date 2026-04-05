using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Features.DeclineFriend;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Friendships;

public static partial class FriendshipsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> DeclineFriend(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<DeclineFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new DeclineFriendCommand(userIdResult.Value, userId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
