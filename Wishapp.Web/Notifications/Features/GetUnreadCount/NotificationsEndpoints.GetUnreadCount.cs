using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    private static async Task<Results<Ok<int>, UnauthorizedHttpResult>> GetUnreadCount(
        ClaimsPrincipal user,
        IQueryHandler<Features.GetUnreadCount.GetUnreadCountQuery, int> handler,
        CancellationToken ct = default)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.GetUnreadCount.GetUnreadCountQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }
}
