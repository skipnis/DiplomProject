using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Gamification.Features.AddGiftBadges;

public sealed class AddGiftBadgesHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
    : ICommandHandler<AddGiftBadgesCommand>
{
    public async Task<Result> HandleAsync(AddGiftBadgesCommand command, CancellationToken ct = default)
    {
        var activeBadgeIds = await db.FulfilledWishBadgeDefinitions
            .Where(b => b.IsActive)
            .Select(b => b.Id)
            .ToListAsync(ct);

        var invalidBadges = command.BadgeTypes.Except(activeBadgeIds).ToList();
        if (invalidBadges.Count > 0)
            return Error.Validation("Wishes.GiftBadges.InvalidBadge", "One or more badge types are invalid or inactive");

        var eligibility = await wishlistsApi.GetGiftBadgeEligibilityAsync(command.WishlistId, command.WishId, ct);

        if (eligibility is null || !eligibility.WishExists)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        var wishOwnerId = eligibility.WishCreatorId ?? eligibility.WishlistOwnerId;
        if (wishOwnerId != command.UserId)
            return Error.Forbidden("Wishes.GiftBadges.Forbidden", "Only the wish owner can give gift badges");

        if (!eligibility.IsFulfilled)
            return Error.Failure("Wishes.GiftBadges.NotFulfilled", "Cannot rate a wish that has not been fulfilled");

        if (!eligibility.FulfilledByReserverId.HasValue)
            return Result.Success();

        var alreadyRated = await db.FulfilledWishBadges
            .AnyAsync(b => b.WishId == command.WishId, ct);

        if (alreadyRated)
            return Error.Conflict("Wishes.GiftBadges.AlreadyRated", "Gift badges have already been given for this wish");

        var gifterId = eligibility.FulfilledByReserverId.Value;

        foreach (var badgeType in command.BadgeTypes)
        {
            db.FulfilledWishBadges.Add(FulfilledWishBadge.Create(
                command.WishId,
                command.UserId,
                gifterId,
                badgeType));
        }

        await db.SaveChangesAsync(ct);

        await RecalculateAchievementsAsync(db, gifterId, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static async Task RecalculateAchievementsAsync(
        ApplicationDbContext db,
        Guid gifterId,
        CancellationToken ct)
    {
        var badgeCounts = await db.FulfilledWishBadges
            .Where(b => b.GifterUserId == gifterId)
            .GroupBy(b => b.BadgeType)
            .Select(g => new { BadgeType = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countsByBadgeType = badgeCounts.ToDictionary(b => b.BadgeType, b => b.Count);
        var uniqueBadgeTypeCount = countsByBadgeType.Count;

        var achievementDefinitions = await db.AchievementDefinitions
            .Where(d => d.IsActive)
            .ToListAsync(ct);

        var existingAchievements = await db.UserAchievements
            .Where(a => a.UserId == gifterId)
            .ToListAsync(ct);

        var achievementsByDefinitionId = existingAchievements.ToDictionary(a => a.DefinitionId);

        foreach (var definition in achievementDefinitions)
        {
            var progress = definition.RuleType switch
            {
                AchievementRuleType.SpecificBadgeCount when definition.LinkedBadgeTypeId.HasValue
                    => countsByBadgeType.GetValueOrDefault(definition.LinkedBadgeTypeId.Value, 0),
                AchievementRuleType.UniqueBadgeTypes
                    => uniqueBadgeTypeCount,
                _ => 0
            };

            if (!achievementsByDefinitionId.TryGetValue(definition.Id, out var achievement))
            {
                achievement = UserAchievement.Create(gifterId, definition.Id);
                db.UserAchievements.Add(achievement);
            }

            achievement.UpdateProgress(progress, definition.Threshold);
        }
    }
}
