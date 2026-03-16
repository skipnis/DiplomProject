using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishDto(
    Guid Id,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? Url,
    string? ImagePath,
    DateTimeOffset CreatedAt)
{
    public static WishDto From(Wish wish) => new(
        wish.Id,
        wish.Name,
        wish.Description,
        wish.Price,
        wish.Currency,
        wish.Priority,
        wish.Url,
        wish.ImagePath,
        wish.CreatedAt);
}