using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWish;

public record AddWishRequest(
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    string? Url,
    WishPriority Priority = WishPriority.None);