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
    DateTimeOffset CreatedAt,
    int FulfilledWishCount,
    List<WishlistMemberDto> Members)
{
    public static WishlistDto From(Wishlist wishlist) => new(
        wishlist.Id,
        wishlist.Name,
        wishlist.Description,
        wishlist.Emoji,
        wishlist.Visibility,
        wishlist.IsSystem,
        wishlist.SystemType,
        wishlist.IsSurpriseModeEnabled,
        wishlist.CreatedAt,
        wishlist.Wishes.Count(w => w.IsFulfilled),
        wishlist.Members.Select(WishlistMemberDto.From).ToList());
}