using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions;

public sealed class AchievementDefinitionRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public AchievementRuleType RuleType { get; init; }
    public int? LinkedBadgeTypeId { get; init; }
    public int Threshold { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
}
