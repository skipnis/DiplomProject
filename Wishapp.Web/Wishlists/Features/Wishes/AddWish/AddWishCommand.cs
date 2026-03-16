using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWish;

public record AddWishCommand(
    Guid WishlistId,
    Guid UserId,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? Url) : ICommand<AddWishResponse>;