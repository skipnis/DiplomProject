namespace Wishapp.Web.Gamification.Entities;

public sealed class UserAchievement
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int DefinitionId { get; private set; }
    public int Progress { get; private set; }
    public bool IsEarned { get; private set; }
    public DateTimeOffset? EarnedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserAchievement() { }

    public static UserAchievement Create(Guid userId, int definitionId)
    {
        var now = DateTimeOffset.UtcNow;
        return new UserAchievement
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            DefinitionId = definitionId,
            Progress = 0,
            IsEarned = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public bool UpdateProgress(int progress, int threshold)
    {
        Progress = progress;
        UpdatedAt = DateTimeOffset.UtcNow;
        if (!IsEarned && progress >= threshold)
        {
            IsEarned = true;
            EarnedAt = DateTimeOffset.UtcNow;
            return true;
        }
        return false;
    }
}
