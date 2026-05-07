using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishlistDto(
    Guid Id,
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility,
    bool IsSystem,
    SystemWishlistType SystemType,
    bool IsSurpriseModeEnabled,
    int FulfilledWishCount,
    List<WishlistMemberDto> Members);