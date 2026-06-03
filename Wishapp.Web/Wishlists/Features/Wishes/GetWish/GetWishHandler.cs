using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWish;

public sealed class GetWishHandler(
    ApplicationDbContext db,
    IReservationsApi reservationsApi,
    IGamificationApi gamificationApi,
    IUsersApi usersApi)
    : IQueryHandler<GetWishQuery, WishDto>
{
    public async Task<Result<WishDto>> HandleAsync(
        GetWishQuery query,
        CancellationToken ct = default)
    {
        var wish = await db.Wishes
            .AsNoTracking()
            .Where(w => w.Id == query.WishId && w.WishlistId == query.WishlistId)
            .FirstOrDefaultAsync(ct);

        if (wish is null)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        var reservedIds = await reservationsApi.GetReservedWishIdsAsync([wish.Id], ct);
        var hasGiftBadges = await gamificationApi.HasGiftBadgesAsync(wish.Id, ct);

        string? createdByDisplayName = null;
        if (wish.CreatedByUserId.HasValue)
        {
            var displayNames = await usersApi.GetUsernamesAsync([wish.CreatedByUserId.Value], ct);
            displayNames.TryGetValue(wish.CreatedByUserId.Value, out createdByDisplayName);
        }

        return new WishDto(
            wish.Id,
            wish.Name,
            wish.Description,
            wish.Price,
            wish.Currency,
            wish.Priority,
            wish.Url,
            wish.ImagePath,
            wish.CreatedAt,
            wish.IsFulfilled,
            wish.FulfilledAt,
            !query.HideReservations && reservedIds.Contains(wish.Id),
            query.IsOwner ? wish.ShareToken : null,
            query.HideReservations ? null : wish.FulfilledByReserverId,
            hasGiftBadges,
            wish.CreatedByUserId,
            createdByDisplayName);
    }
}
