using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.UpdateWishlist;

public record UpdateWishlistRequest(
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility);