using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Reservations.Dtos;

public record MyReservationDto(
    Guid ReservationId,
    Guid WishId,
    Guid WishlistId,
    string WishName,
    string? WishImagePath,
    decimal? WishPrice,
    Currency? WishCurrency,
    string WishlistName,
    string WishlistOwnerName,
    DateTimeOffset ReservedAt);
