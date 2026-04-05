using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;

public record CreateWishlistRequest(
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility,
    bool IsSurpriseModeEnabled = false,
    List<WishlistMemberInvite>? Members = null);