namespace Wishapp.Web.Gamification.Dtos;

public record GiftProfileDto(
    int GiftsGiven,
    int GiftsWithBadges,
    double HitRate,
    int Level,
    string LevelName,
    int NextLevelThreshold,
    IReadOnlyList<UserAchievementDto> Achievements,
    IReadOnlyList<BadgeCountDto> BadgesReceived);

public record UserAchievementDto(
    int DefinitionId,
    string Name,
    string Description,
    string Emoji,
    int Progress,
    int Threshold,
    bool IsEarned,
    DateTimeOffset? EarnedAt);

public record BadgeCountDto(int BadgeType, string Emoji, string Label, int Count);
