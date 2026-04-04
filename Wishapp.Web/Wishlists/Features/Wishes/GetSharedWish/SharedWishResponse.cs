using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetSharedWish;

public record SharedWishResponse(
    Guid Id,
    Guid WishlistId,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? Url,
    string? ImagePath,
    bool IsFulfilled,
    bool IsReserved,
    WishlistVisibility WishlistVisibility,
    string OwnerUsername);
