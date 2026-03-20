using Wishapp.Web.Infrastructure.GoogleCalendar;

namespace Wishapp.Web.Events;

public static class EventsModule
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services)
    {
        services.AddScoped<IEventsApi, EventsApi>();
        
        services.AddHttpClient<IGoogleCalendarService, GoogleCalendarService>();
        
        return services;
    }
}
