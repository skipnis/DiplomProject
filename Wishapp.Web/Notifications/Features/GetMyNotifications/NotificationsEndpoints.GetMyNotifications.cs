using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Notifications.Dtos;

namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<NotificationDto>>, UnauthorizedHttpResult>> GetMyNotifications(
        ClaimsPrincipal user,
        IQueryHandler<Features.GetMyNotifications.GetMyNotificationsQuery, PagedResponse<NotificationDto>> handler,
        int page = 1,
        int pageSize = 20,
        DateOnly? from = null,
        DateOnly? to = null,
        bool? isRead = null,
        CancellationToken ct = default)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.GetMyNotifications.GetMyNotificationsQuery(userIdResult.Value, new PagedRequest(page, pageSize), from, to, isRead), ct);

        return TypedResults.Ok(result.Value);
    }
}
