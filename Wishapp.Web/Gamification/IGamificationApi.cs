using Wishapp.Web.Gamification.Dtos;

namespace Wishapp.Web.Gamification;

public interface IGamificationApi
{
    Task<Dictionary<Guid, List<CatalogItemBadgeDto>>> GetBadgesForItemsAsync(
        IReadOnlyList<Guid> itemIds,
        Guid? userId,
        CancellationToken ct = default);
    Task<bool> HasGiftBadgesAsync(Guid wishId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetWishIdsWithBadgesAsync(IReadOnlyList<Guid> wishIds, CancellationToken ct = default);
    Task DeleteBadgesForWishAsync(Guid wishId, CancellationToken ct = default);
    Task DeleteBadgesForWishesAsync(IReadOnlyList<Guid> wishIds, CancellationToken ct = default);
    Task RecalculateAchievementsAsync(Guid userId, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
