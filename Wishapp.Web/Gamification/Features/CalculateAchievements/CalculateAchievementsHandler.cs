using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Proposals;

namespace Wishapp.Web.Gamification.Features.CalculateAchievements;

public sealed class CalculateAchievementsHandler(
    ApplicationDbContext db,
    IProposalsApi proposalsApi,
    INotificationsApi notificationsApi)
    : ICommandHandler<CalculateAchievementsCommand>
{
    public async Task<Result> HandleAsync(CalculateAchievementsCommand command, CancellationToken ct = default)
    {
        var badgeCounts = await db.FulfilledWishBadges
            .Where(badge => badge.GifterUserId == command.UserId)
            .GroupBy(badge => badge.BadgeType)
            .Select(group => new { BadgeType = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var countsByBadgeType = badgeCounts.ToDictionary(badge => badge.BadgeType, badge => badge.Count);
        var uniqueBadgeTypeCount = countsByBadgeType.Count;

        var likedProposalsCount = await proposalsApi.GetLikedProposalsCountAsync(command.UserId, ct);

        var achievementDefinitions = await db.AchievementDefinitions
            .Where(definition => definition.IsActive)
            .ToListAsync(ct);

        var existingAchievements = await db.UserAchievements
            .Where(achievement => achievement.UserId == command.UserId)
            .ToListAsync(ct);

        var achievementsByDefinitionId = existingAchievements.ToDictionary(achievement => achievement.DefinitionId);

        foreach (var definition in achievementDefinitions)
        {
            var progress = definition.RuleType switch
            {
                AchievementRuleType.SpecificBadgeCount when definition.LinkedBadgeTypeId.HasValue
                    => countsByBadgeType.GetValueOrDefault(definition.LinkedBadgeTypeId.Value, 0),
                AchievementRuleType.UniqueBadgeTypes
                    => uniqueBadgeTypeCount,
                AchievementRuleType.LikedProposalsCount
                    => likedProposalsCount,
                _ => 0
            };

            if (!achievementsByDefinitionId.TryGetValue(definition.Id, out var achievement))
            {
                achievement = UserAchievement.Create(command.UserId, definition.Id);
                db.UserAchievements.Add(achievement);
            }

            var justEarned = achievement.UpdateProgress(progress, definition.Threshold);

            if (justEarned)
            {
                await notificationsApi.EnqueueAsync(command.UserId, NotificationType.AchievementEarned, new
                {
                    achievementId = definition.Id,
                    name = definition.Name,
                    emoji = definition.Emoji
                }, ct);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
