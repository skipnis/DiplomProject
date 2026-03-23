namespace Wishapp.Web.Catalog.Api;

public interface ICatalogApi
{
    Task<CatalogItemData?> GetCatalogItemDataAsync(Guid id, CancellationToken ct = default);
}
