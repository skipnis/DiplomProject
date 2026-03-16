using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;

public record CreateWishlistResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility,
    DateTimeOffset CreatedAt);