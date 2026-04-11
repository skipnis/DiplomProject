using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishlistSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility,
    bool IsSystem,
    SystemWishlistType SystemType,
    int WishCount,
    int FulfilledWishCount,
    DateTimeOffset CreatedAt);