namespace Wishapp.Web.Infrastructure.Minio;

public static class StoragePaths
{
    public static string WishImage(Guid wishlistId, Guid wishId) =>
        $"wishes/{wishlistId}/{wishId}/image";

    public static string CatalogItemImage(Guid itemId) =>
        $"catalog/items/{itemId}/image";

    public static string CatalogCollectionCover(Guid collectionId) =>
        $"catalog/collections/{collectionId}/cover";
}