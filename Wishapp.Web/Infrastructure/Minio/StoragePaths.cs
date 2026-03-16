namespace Wishapp.Web.Infrastructure.Minio;

public static class StoragePaths
{
    public static string WishImage(Guid wishlistId, Guid wishId) =>
        $"wishes/{wishlistId}/{wishId}/image";

    public static string UserAvatar(Guid userId) =>
        $"users/{userId}/avatar";

    public static string UserCover(Guid userId) =>
        $"users/{userId}/cover";
}