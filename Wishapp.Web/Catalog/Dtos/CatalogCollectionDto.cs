namespace Wishapp.Web.Catalog.Dtos;

public record CatalogCollectionDto(
    Guid Id,
    string Name,
    string? Description,
    OccasionDto? Occasion,
    string? CoverImagePath,
    int Order,
    List<CatalogItemSummaryDto> Items);
