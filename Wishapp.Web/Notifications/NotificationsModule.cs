using Wishapp.Web.Notifications.Features.DispatchPendingNotifications;
using Wishapp.Web.Notifications.SignalR;

namespace Wishapp.Web.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<INotificationsApi, NotificationsApi>();
        services.AddScoped<INotificationSender, SignalRNotificationSender>();
        services.AddHostedService<NotificationDispatchWorker>();

        return services;
    }
}
