using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions;

public record AchievementDefinitionAdminDto(
    int Id,
    string Name,
    string Description,
    string Emoji,
    AchievementRuleType RuleType,
    int? LinkedBadgeTypeId,
    int Threshold,
    int Order,
    bool IsActive);
