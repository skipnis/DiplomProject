using System.Security.Claims;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Notifications.Dtos;
using Wishapp.Web.Notifications.Sse;

namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/notifications").RequireAuthorization();

        notifications.MapGet("/stream", StreamNotifications);
        notifications.MapGet("/my", GetMyNotifications);
        notifications.MapGet("/unread-count", GetUnreadCount);
        notifications.MapPatch("/{id:guid}/read", MarkAsRead);
        notifications.MapPatch("/read-all", MarkAllAsRead);
        notifications.MapDelete("/{id:guid}", DeleteNotification);

        return app;
    }

    private static IResult StreamNotifications(
        ClaimsPrincipal user,
        SseConnectionManager connections,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var (connectionId, channel) = connections.Register(userIdResult.Value);
        ct.Register(() => connections.Unregister(userIdResult.Value, connectionId));
        return TypedResults.ServerSentEvents(channel.Reader.ReadAllAsync(ct), eventType: "notification");
    }
}
