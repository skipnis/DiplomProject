using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification.Features.AchievementChecker;

public sealed class AchievementCheckerWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AchievementCheckerWorker> logger) : BackgroundService
{
    private DateTimeOffset _watermark = DateTimeOffset.UtcNow.AddSeconds(-35);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessNewBadgesAsync(stoppingToken);
        }
    }

    private async Task ProcessNewBadgesAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = _watermark;
        _watermark = DateTimeOffset.UtcNow;

        var gifterIds = await db.FulfilledWishBadges
            .Where(b => b.CreatedAt > cutoff)
            .Select(b => b.GifterUserId)
            .Distinct()
            .ToListAsync(ct);

        if (gifterIds.Count == 0)
            return;

        var achievementDefinitions = await db.AchievementDefinitions
            .Where(d => d.IsActive)
            .ToListAsync(ct);

        foreach (var gifterId in gifterIds)
        {
            await UpdateAchievementsAsync(db, gifterId, achievementDefinitions, ct);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Checked achievements for {Count} gifters", gifterIds.Count);
    }

    private static async Task UpdateAchievementsAsync(
        ApplicationDbContext db,
        Guid gifterId,
        List<AchievementDefinition> definitions,
        CancellationToken ct)
    {
        var badgeCounts = await db.FulfilledWishBadges
            .Where(b => b.GifterUserId == gifterId)
            .GroupBy(b => b.BadgeType)
            .Select(g => new { BadgeType = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countsByBadgeType = badgeCounts.ToDictionary(b => b.BadgeType, b => b.Count);
        var uniqueBadgeTypeCount = countsByBadgeType.Count;

        var existingAchievements = await db.UserAchievements
            .Where(a => a.UserId == gifterId)
            .ToListAsync(ct);

        var achievementsByDefinitionId = existingAchievements.ToDictionary(a => a.DefinitionId);

        foreach (var definition in definitions)
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
