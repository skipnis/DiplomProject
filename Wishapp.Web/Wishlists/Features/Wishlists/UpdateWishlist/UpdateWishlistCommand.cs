using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.UpdateWishlist;

public record UpdateWishlistCommand(
    Guid WishlistId,
    Guid UserId,
    string Name,
    string? Description,
    string? Emoji,
    WishlistVisibility Visibility) : ICommand;
    