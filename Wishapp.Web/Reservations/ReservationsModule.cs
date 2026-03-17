namespace Wishapp.Web.Reservations;

public static class ReservationsModule
{
    public static IServiceCollection AddReservationsModule(this IServiceCollection services)
    {
        services.AddScoped<IReservationsApi, ReservationsApi>();

        return services;
    }
}
