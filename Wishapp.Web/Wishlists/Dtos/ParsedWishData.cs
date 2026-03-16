namespace Wishapp.Web.Wishlists.Dtos;

public record ParsedWishData(
    string? Name,
    string? Description,
    decimal? Price,
    string? Currency,
    string? ExternalImageUrl);