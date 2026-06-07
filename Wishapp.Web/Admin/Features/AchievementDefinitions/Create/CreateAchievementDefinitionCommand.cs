using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Create;

public record CreateAchievementDefinitionCommand(
    string Name,
    string Description,
    string Emoji,
    AchievementRuleType RuleType,
    int? LinkedBadgeTypeId,
    int Threshold) : ICommand<int>;
