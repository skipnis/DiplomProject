namespace Wishapp.Web.Events;

public static class EventsModule
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services)
    {
        services.AddScoped<IEventsApi, EventsApi>();

        return services;
    }
}
