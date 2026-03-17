using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.UnfulfillWish;

public record UnfulfillWishCommand(Guid WishlistId, Guid WishId) : ICommand;
