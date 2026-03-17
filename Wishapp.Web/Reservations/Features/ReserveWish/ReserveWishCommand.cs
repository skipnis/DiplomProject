using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Reservations.Features.ReserveWish;

public record ReserveWishCommand(Guid WishId, Guid WishlistId, Guid UserId) : ICommand;
