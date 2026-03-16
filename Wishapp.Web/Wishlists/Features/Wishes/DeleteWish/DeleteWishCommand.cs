using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.DeleteWish;

public record DeleteWishCommand(Guid WishlistId, Guid WishId) : ICommand;