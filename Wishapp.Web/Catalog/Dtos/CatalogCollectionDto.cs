namespace Wishapp.Web.Catalog.Dtos;

public record CatalogCollectionDto(
    Guid Id,
    string Name,
    string? Description,
    string? Occasion,
    string? CoverImagePath,
    int Order,
    List<CatalogItemDto> Items);
