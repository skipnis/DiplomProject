using Wishapp.Web.Infrastructure.Validation;
using Wishapp.Web.Reservations.Features.ReserveWish;

namespace Wishapp.Web.Reservations;

public static partial class ReservationsEndpoints
{
    public static IEndpointRouteBuilder MapReservationsEndpoints(this IEndpointRouteBuilder app)
    {
        var reservations = app.MapGroup("/reservations").RequireAuthorization();

        reservations.MapPost("/{wishId:guid}", ReserveWish)
            .AddEndpointFilter<ValidationFilter<ReserveWishRequest>>();

        reservations.MapDelete("/{wishId:guid}", CancelReservation);

        reservations.MapGet("/my", GetMyReservations);

        reservations.MapGet("/my-wishes", GetReservationsOnMyWishes);

        return app;
    }
}
