using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Reservations.Dtos;

public record WishReservedOnMyWishDto(
    Guid WishId,
    Guid WishlistId,
    string WishName,
    string WishlistName,
    string? WishImagePath,
    decimal? WishPrice,
    Currency? WishCurrency,
    Guid ReservedByUserId,
    string ReservedByDisplayName,
    DateTimeOffset ReservedAt);
