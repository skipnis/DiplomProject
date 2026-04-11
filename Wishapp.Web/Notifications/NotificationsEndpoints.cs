namespace Wishapp.Web.Notifications;

public static partial class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/notifications").RequireAuthorization();

        notifications.MapGet("/my", GetMyNotifications);
        notifications.MapGet("/unread-count", GetUnreadCount);
        notifications.MapPatch("/{id:guid}/read", MarkAsRead);
        notifications.MapPatch("/read-all", MarkAllAsRead);

        return app;
    }
}
