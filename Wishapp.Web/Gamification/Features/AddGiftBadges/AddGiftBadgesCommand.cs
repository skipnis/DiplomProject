using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Gamification.Features.AddGiftBadges;

public record AddGiftBadgesCommand(
    Guid UserId,
    Guid WishlistId,
    Guid WishId,
    IReadOnlyList<int> BadgeTypes) : ICommand;
