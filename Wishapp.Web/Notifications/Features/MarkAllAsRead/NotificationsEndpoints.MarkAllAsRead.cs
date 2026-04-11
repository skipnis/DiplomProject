using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    private static async Task<Results<NoContent, UnauthorizedHttpResult>> MarkAllAsRead(
        ClaimsPrincipal user,
        ICommandHandler<Features.MarkAllAsRead.MarkAllAsReadCommand> handler,
        CancellationToken ct = default)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        await handler.HandleAsync(
            new Features.MarkAllAsRead.MarkAllAsReadCommand(userIdResult.Value), ct);

        return TypedResults.NoContent();
    }
}
