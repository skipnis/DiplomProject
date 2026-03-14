namespace Wishapp.Web.Reservations;

public static class ReservationsEndpoints
{
    public static IEndpointRouteBuilder MapReservationsEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var reservationsEndpoints = routeBuilder.MapGroup("/reservations");
        return routeBuilder;
    }
}