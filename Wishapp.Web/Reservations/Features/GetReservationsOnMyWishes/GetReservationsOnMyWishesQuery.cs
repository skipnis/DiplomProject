using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Reservations.Dtos;

namespace Wishapp.Web.Reservations.Features.GetReservationsOnMyWishes;

public record GetReservationsOnMyWishesQuery(Guid UserId) : IQuery<List<WishReservedOnMyWishDto>>;
