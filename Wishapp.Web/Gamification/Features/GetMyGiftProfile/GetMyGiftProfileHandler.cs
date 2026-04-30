using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Gamification.Features.GetMyGiftProfile;

public sealed class GetMyGiftProfileHandler(
    ApplicationDbContext db,
    IOptions<GiftLevelOptions> levelOptions,
    IWishlistsApi wishlistsApi)
    : IQueryHandler<GetMyGiftProfileQuery, GiftProfileDto>
{
    public async Task<Result<GiftProfileDto>> HandleAsync(
        GetMyGiftProfileQuery query,
        CancellationToken ct = default)
    {
        var wishStats = await wishlistsApi.GetUserWishStatsAsync(query.UserId, ct);
        var giftsGiven = wishStats.GiftedCount;

        var giftsWithBadges = await db.FulfilledWishBadges
            .Where(b => b.GifterUserId == query.UserId)
            .Select(b => b.WishId)
            .Distinct()
            .CountAsync(ct);

        var hitRate = giftsGiven > 0 ? (double)giftsWithBadges / giftsGiven : 0;

        var (level, levelName, nextLevelThreshold) = levelOptions.Value.Calculate(giftsGiven);

        var achievementDefinitions = await db.AchievementDefinitions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Order)
            .ToListAsync(ct);

        var userAchievements = await db.UserAchievements
            .Where(a => a.UserId == query.UserId)
            .ToListAsync(ct);

        var achievementsByDefinitionId = userAchievements.ToDictionary(a => a.DefinitionId);

        var achievements = achievementDefinitions.Select(def =>
        {
            achievementsByDefinitionId.TryGetValue(def.Id, out var userAchievement);
            return new UserAchievementDto(
                def.Id, def.Name, def.Description, def.Emoji,
                userAchievement?.Progress ?? 0, def.Threshold,
                userAchievement?.IsEarned ?? false, userAchievement?.EarnedAt);
        }).ToList();

        var fulfilledBadgeDefinitions = await db.FulfilledWishBadgeDefinitions.ToListAsync(ct);
        var badgeInfoById = fulfilledBadgeDefinitions.ToDictionary(b => b.Id, b => (b.Emoji, b.Label));

        var badgesReceived = await db.FulfilledWishBadges
            .Where(b => b.GifterUserId == query.UserId)
            .GroupBy(b => b.BadgeType)
            .Select(g => new { BadgeType = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var badgeCountDtos = badgesReceived
            .Select(b =>
            {
                var info = badgeInfoById.GetValueOrDefault(b.BadgeType);
                return new BadgeCountDto(b.BadgeType, info.Emoji ?? "", info.Label ?? $"Бейдж #{b.BadgeType}", b.Count);
            })
            .OrderBy(b => b.BadgeType)
            .ToList();

        return new GiftProfileDto(
            giftsGiven, giftsWithBadges, hitRate,
            level, levelName, nextLevelThreshold,
            achievements, badgeCountDtos);
    }
}
