using Wishapp.Web.Catalog.Dtos;

namespace Wishapp.Web.Catalog;

public interface ICatalogApi
{
    Task<CatalogItemData?> GetCatalogItemDataAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, CatalogItemData>> GetCatalogItemsDataAsync(List<Guid> ids, CancellationToken ct = default);
    Task IncrementWishCountAsync(Guid id, CancellationToken ct = default);
    Task<bool> ItemExistsAsync(Guid id, CancellationToken ct = default);
}
