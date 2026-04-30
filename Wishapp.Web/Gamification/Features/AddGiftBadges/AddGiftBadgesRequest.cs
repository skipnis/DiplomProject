namespace Wishapp.Web.Gamification.Features.AddGiftBadges;

public sealed class AddGiftBadgesRequest
{
    public IReadOnlyList<int> BadgeTypes { get; init; } = [];
}
