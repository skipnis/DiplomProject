using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists;

public record WishNotificationData(
    string WishName,
    Guid WishlistId,
    string WishlistName,
    Guid OwnerId,
    bool IsSurpriseModeEnabled);

public record GiftBadgeEligibilityData(
    Guid WishlistOwnerId,
    bool WishExists,
    bool IsFulfilled,
    Guid? FulfilledByReserverId);

public interface IWishlistsApi
{
    Task CreateSystemWishlistsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> WishExistsAsync(Guid wishId, CancellationToken ct = default);
    Task<bool> CanAccessWishlistAsync(Guid userId, Guid wishlistId, CancellationToken ct = default);
    Task<WishlistAccessData?> GetWishlistAccessDataAsync(Guid wishlistId, CancellationToken ct = default);
    Task<bool> IsWishFulfilledAsync(Guid wishId, CancellationToken ct = default);
    Task<List<WishSummary>> GetWishesSummaryAsync(List<Guid> wishIds, CancellationToken ct = default);
    Task<Result> CanLinkWishlistAsync(Guid userId, Guid wishlistId, CancellationToken ct = default);
    Task<UserWishStats> GetUserWishStatsAsync(Guid userId, CancellationToken ct = default);
    Task<WishNotificationData?> GetWishNotificationDataAsync(Guid wishId, CancellationToken ct = default);
    Task<GiftBadgeEligibilityData?> GetGiftBadgeEligibilityAsync(Guid wishlistId, Guid wishId, CancellationToken ct = default);
    Task<List<PublicFulfilledWishDto>> GetPublicFulfilledWishesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetNonSurpriseWishlistIdsByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}

public record UserWishStats(int ReceivedCount, int GiftedCount);

public record PublicFulfilledWishDto(
    Guid Id,
    string WishName,
    string? ImagePath,
    string WishlistName,
    DateTimeOffset FulfilledAt);
