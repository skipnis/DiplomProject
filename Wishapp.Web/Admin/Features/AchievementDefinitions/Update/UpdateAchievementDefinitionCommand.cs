using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Update;

public record UpdateAchievementDefinitionCommand(
    int Id,
    string Name,
    string Description,
    string Emoji,
    AchievementRuleType RuleType,
    int? LinkedBadgeTypeId,
    int Threshold,
    bool IsActive) : ICommand;
