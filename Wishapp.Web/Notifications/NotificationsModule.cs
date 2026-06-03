using Wishapp.Web.Notifications.Sse;

namespace Wishapp.Web.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddSingleton<SseConnectionManager>();
        services.AddScoped<INotificationsApi, NotificationsApi>();
        services.AddScoped<INotificationSender, SseNotificationSender>();

        return services;
    }
}
