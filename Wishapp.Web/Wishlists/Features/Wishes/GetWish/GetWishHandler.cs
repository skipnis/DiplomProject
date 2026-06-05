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

        var userIdsToResolve = new List<Guid>();
        if (wish.CreatedByUserId.HasValue) userIdsToResolve.Add(wish.CreatedByUserId.Value);
        if (wish.FulfilledByReserverId.HasValue) userIdsToResolve.Add(wish.FulfilledByReserverId.Value);

        var displayNames = userIdsToResolve.Count > 0
            ? await usersApi.GetUsernamesAsync(userIdsToResolve, ct)
            : new Dictionary<Guid, string>();

        displayNames.TryGetValue(wish.CreatedByUserId ?? Guid.Empty, out var createdByDisplayName);
        displayNames.TryGetValue(wish.FulfilledByReserverId ?? Guid.Empty, out var fulfilledByDisplayName);

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
            !query.HideActiveReservations && reservedIds.Contains(wish.Id),
            query.IsOwner ? wish.ShareToken : null,
            wish.FulfilledByReserverId,
            fulfilledByDisplayName,
            hasGiftBadges,
            wish.CreatedByUserId,
            createdByDisplayName);
    }
}
