using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Features.MakeFriend;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Friendships;

public static partial class FriendshipsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, Conflict<Error>, UnauthorizedHttpResult, BadRequest<Error>>> MakeFriend(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<MakeFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new MakeFriendCommand(userIdResult.Value, userId), ct);

        if (result.IsSuccess) return TypedResults.NoContent();

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Conflict => TypedResults.Conflict(result.Error),
            _ => TypedResults.BadRequest(result.Error)
        };
    }
}
