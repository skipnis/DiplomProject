using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;

namespace Wishapp.Web.Gamification.Features.GetGiftBadges;

public record GetGiftBadgesQuery(Guid WishlistId, Guid WishId) : IQuery<List<FulfilledWishBadgeDto>>;
