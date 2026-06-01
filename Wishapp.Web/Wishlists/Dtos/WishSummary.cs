namespace Wishapp.Web.Wishlists.Dtos;

public record WishSummary(
    Guid WishId,
    Guid WishlistId,
    string WishName,
    string? ImagePath,
    decimal? Price,
    Currency? Currency,
    string WishlistName,
    Guid OwnerId);
