namespace Wishapp.Web.Infrastructure.ObjectStorage;

public static class StoragePaths
{
    public static string WishImage(Guid wishlistId, Guid wishId) =>
        $"wishes/{wishlistId}/{wishId}/image";

    public static string CatalogItemImage(Guid itemId) =>
        $"catalog/items/{itemId}/image";

    public static string CatalogCollectionCover(Guid collectionId) =>
        $"catalog/collections/{collectionId}/cover";

    public static string UserAvatar(Guid userId) =>
        $"users/{userId}/avatar";

    public static string ProposalCustomImage(Guid proposalId) =>
        $"proposals/{proposalId}/image";
}
