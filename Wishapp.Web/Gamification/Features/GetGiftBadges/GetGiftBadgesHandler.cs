using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Gamification.Features.GetGiftBadges;

public sealed class GetGiftBadgesHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
    : IQueryHandler<GetGiftBadgesQuery, List<FulfilledWishBadgeDto>>
{
    public async Task<Result<List<FulfilledWishBadgeDto>>> HandleAsync(
        GetGiftBadgesQuery query,
        CancellationToken ct = default)
    {
        var wishExists = await wishlistsApi.WishExistsAsync(query.WishId, ct);

        if (!wishExists)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        return await db.FulfilledWishBadges
            .Where(b => b.WishId == query.WishId)
            .OrderBy(b => b.CreatedAt)
            .Select(b => new FulfilledWishBadgeDto(b.BadgeType, b.CreatedAt))
            .ToListAsync(ct);
    }
}
