using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Catalog.Api;

public record CatalogItemData(
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    string? ImagePath,
    string? Url);
