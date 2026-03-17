namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    public static IEndpointRouteBuilder MapReservationsEndpoints(this IEndpointRouteBuilder app)
    {
        var reservations = app.MapGroup("/reservations").RequireAuthorization();

        reservations.MapPost("/{wishId:guid}", ReserveWish);

        reservations.MapDelete("/{wishId:guid}", CancelReservation);

        reservations.MapGet("/my", GetMyReservations);

        return app;
    }
}
