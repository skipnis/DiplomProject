namespace Wishapp.Web.Catalog.Api;

public interface ICatalogApi
{
    Task<CatalogItemData?> GetCatalogItemDataAsync(Guid id, CancellationToken ct = default);
    Task IncrementWishCountAsync(Guid id, CancellationToken ct = default);
    Task<bool> ItemExistsAsync(Guid id, CancellationToken ct = default);
}
