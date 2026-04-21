namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal? Price,
    string? Currency,
    string? ImagePath,
    string? Url,
    Guid CategoryId,
    string CategoryName,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    double? AverageRating,
    int RatingCount,
    int? MyRating,
    int WishCount,
    string? CollectionItemDescription);
