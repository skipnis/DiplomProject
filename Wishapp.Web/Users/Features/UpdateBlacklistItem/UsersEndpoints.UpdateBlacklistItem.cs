using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.UpdateBlacklistItem;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> UpdateBlacklistItem(
        [FromRoute] Guid itemId,
        UpdateBlacklistItemRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UpdateBlacklistItemCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new UpdateBlacklistItemCommand(userIdResult.Value, itemId, request.Title), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
