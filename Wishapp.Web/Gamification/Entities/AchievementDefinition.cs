namespace Wishapp.Web.Gamification.Entities;

public sealed class AchievementDefinition
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Emoji { get; private set; } = string.Empty;
    public AchievementRuleType RuleType { get; private set; }
    public int? LinkedBadgeTypeId { get; private set; }
    public int Threshold { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; }

    private AchievementDefinition() { }

    public static AchievementDefinition Create(
        string name, string description, string emoji,
        AchievementRuleType ruleType, int? linkedBadgeTypeId,
        int threshold, int order)
    {
        return new AchievementDefinition
        {
            Name = name, Description = description, Emoji = emoji,
            RuleType = ruleType, LinkedBadgeTypeId = linkedBadgeTypeId,
            Threshold = threshold, Order = order, IsActive = true
        };
    }

    public void Update(
        string name, string description, string emoji,
        AchievementRuleType ruleType, int? linkedBadgeTypeId,
        int threshold, int order, bool isActive)
    {
        Name = name; Description = description; Emoji = emoji;
        RuleType = ruleType; LinkedBadgeTypeId = linkedBadgeTypeId;
        Threshold = threshold; Order = order; IsActive = isActive;
    }
}
