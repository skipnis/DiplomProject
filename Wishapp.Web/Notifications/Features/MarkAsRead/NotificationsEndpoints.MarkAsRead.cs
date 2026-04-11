using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> MarkAsRead(
        Guid id,
        ClaimsPrincipal user,
        ICommandHandler<Features.MarkAsRead.MarkAsReadCommand> handler,
        CancellationToken ct = default)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.MarkAsRead.MarkAsReadCommand(id, userIdResult.Value), ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.Forbid(),
            };
        }

        return TypedResults.NoContent();
    }
}
