namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemData(
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    string? ImagePath,
    string? Url);
