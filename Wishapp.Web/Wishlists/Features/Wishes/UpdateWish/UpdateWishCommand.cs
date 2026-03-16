using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.UpdateWish;

public record UpdateWishCommand(
    Guid WishlistId,
    Guid WishId,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? Url) : ICommand;