using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions;

public record AchievementDefinitionRequest(
    string Name,
    string Description,
    string Emoji,
    AchievementRuleType RuleType,
    int? LinkedBadgeTypeId,
    int Threshold,
    bool IsActive = true);
