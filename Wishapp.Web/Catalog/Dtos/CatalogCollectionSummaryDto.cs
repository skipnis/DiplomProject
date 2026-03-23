namespace Wishapp.Web.Catalog.Dtos;

public record CatalogCollectionSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Occasion,
    string? CoverImagePath,
    int Order,
    int ItemCount);
