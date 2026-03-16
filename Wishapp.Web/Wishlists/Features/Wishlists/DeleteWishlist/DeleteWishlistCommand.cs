using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DeleteWishlist;

public record DeleteWishlistCommand(Guid WishlistId) : ICommand;

