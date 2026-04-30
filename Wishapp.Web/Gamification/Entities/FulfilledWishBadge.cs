namespace Wishapp.Web.Gamification.Entities;

public sealed class FulfilledWishBadge
{
    public Guid Id { get; private set; }
    public Guid WishId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid GifterUserId { get; private set; }
    public int BadgeType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private FulfilledWishBadge() { }

    public static FulfilledWishBadge Create(
        Guid wishId,
        Guid recipientUserId,
        Guid gifterUserId,
        int badgeType)
    {
        return new FulfilledWishBadge
        {
            Id = Guid.CreateVersion7(),
            WishId = wishId,
            RecipientUserId = recipientUserId,
            GifterUserId = gifterUserId,
            BadgeType = badgeType,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
