namespace Wishapp.Web.Gamification.Entities;

public sealed class CatalogItemBadgeVote
{
    public Guid Id { get; private set; }
    public Guid CatalogItemId { get; private set; }
    public Guid UserId { get; private set; }
    public int BadgeType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CatalogItemBadgeVote() { }

    public static CatalogItemBadgeVote Create(Guid catalogItemId, Guid userId, int badgeType)
    {
        return new CatalogItemBadgeVote
        {
            Id = Guid.CreateVersion7(),
            CatalogItemId = catalogItemId,
            UserId = userId,
            BadgeType = badgeType,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
