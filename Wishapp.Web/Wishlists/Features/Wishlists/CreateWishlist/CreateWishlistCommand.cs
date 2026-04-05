using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;

public record CreateWishlistCommand(
    Guid OwnerId,
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility,
    bool IsSurpriseModeEnabled,
    List<WishlistMemberInvite>? Members) : ICommand<CreateWishlistResponse>;