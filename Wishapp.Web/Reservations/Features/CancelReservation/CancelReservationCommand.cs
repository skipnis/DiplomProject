using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Reservations.Features.CancelReservation;

public record CancelReservationCommand(Guid WishId, Guid UserId) : ICommand;
