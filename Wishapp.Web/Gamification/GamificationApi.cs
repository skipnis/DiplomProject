using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Gamification.Features.CalculateAchievements;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification;

public sealed class GamificationApi(
    ApplicationDbContext db,
    ICommandHandler<CalculateAchievementsCommand> calculateAchievementsHandler)
    : IGamificationApi
{
    public async Task<Dictionary<Guid, List<CatalogItemBadgeDto>>> GetBadgesForItemsAsync(
        IReadOnlyList<Guid> itemIds,
        Guid? userId,
        CancellationToken ct = default)
    {
        if (itemIds.Count == 0)
            return [];

        var badgeVotes = await db.CatalogItemBadgeVotes
            .Where(vote => itemIds.Contains(vote.CatalogItemId))
            .GroupBy(vote => new { vote.CatalogItemId, vote.BadgeType })
            .Select(group => new { group.Key.CatalogItemId, group.Key.BadgeType, Count = group.Count() })
            .ToListAsync(ct);

        var myVotes = userId.HasValue
            ? await db.CatalogItemBadgeVotes
                .Where(vote => itemIds.Contains(vote.CatalogItemId) && vote.UserId == userId.Value)
                .Select(vote => new { vote.CatalogItemId, vote.BadgeType })
                .ToListAsync(ct)
            : [];

        var badgeTypeIds = badgeVotes.Select(vote => vote.BadgeType).Distinct().ToList();
        var definitionsByBadgeType = await db.CatalogBadgeDefinitions
            .Where(definition => badgeTypeIds.Contains(definition.Id))
            .Select(definition => new { definition.Id, definition.Emoji, definition.Slug, definition.Label })
            .ToListAsync(ct);
        var definitionMap = definitionsByBadgeType.ToDictionary(definition => definition.Id);

        var myVotesByItemId = myVotes
            .GroupBy(vote => vote.CatalogItemId)
            .ToDictionary(group => group.Key, group => group.Select(vote => vote.BadgeType).ToHashSet());

        return badgeVotes
            .GroupBy(vote => vote.CatalogItemId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var myVotesForItem = myVotesByItemId.GetValueOrDefault(group.Key, []);
                    return group.Select(badge =>
                    {
                        var definition = definitionMap.GetValueOrDefault(badge.BadgeType);
                        return new CatalogItemBadgeDto(
                            badge.BadgeType,
                            definition?.Emoji ?? string.Empty,
                            definition?.Slug ?? string.Empty,
                            definition?.Label ?? string.Empty,
                            badge.Count,
                            myVotesForItem.Contains(badge.BadgeType)
                        );
                    }).ToList();
                });
    }

    public Task RecalculateAchievementsAsync(Guid userId, CancellationToken ct = default)
        => calculateAchievementsHandler.HandleAsync(new CalculateAchievementsCommand(userId), ct);

    public Task<bool> HasGiftBadgesAsync(Guid wishId, CancellationToken ct = default) =>
        db.FulfilledWishBadges.AnyAsync(badge => badge.WishId == wishId, ct);

    public async Task<HashSet<Guid>> GetWishIdsWithBadgesAsync(
        IReadOnlyList<Guid> wishIds,
        CancellationToken ct = default)
    {
        var ids = await db.FulfilledWishBadges
            .Where(badge => wishIds.Contains(badge.WishId))
            .Select(badge => badge.WishId)
            .Distinct()
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task DeleteBadgesForWishAsync(Guid wishId, CancellationToken ct = default)
    {
        await db.FulfilledWishBadges
            .Where(badge => badge.WishId == wishId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteBadgesForWishesAsync(IReadOnlyList<Guid> wishIds, CancellationToken ct = default)
    {
        if (wishIds.Count == 0)
            return;

        await db.FulfilledWishBadges
            .Where(badge => wishIds.Contains(badge.WishId))
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        await db.FulfilledWishBadges
            .Where(badge => badge.GifterUserId == userId)
            .ExecuteDeleteAsync(ct);

        await db.UserAchievements
            .Where(achievement => achievement.UserId == userId)
            .ExecuteDeleteAsync(ct);

        await db.CatalogItemBadgeVotes
            .Where(vote => vote.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
