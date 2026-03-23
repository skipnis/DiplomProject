namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemsFilter(
    Guid? CategoryId = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);
