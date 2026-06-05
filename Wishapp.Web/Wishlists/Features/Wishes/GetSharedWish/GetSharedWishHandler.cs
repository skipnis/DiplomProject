using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations;
using Wishapp.Web.Users;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetSharedWish;

public sealed class GetSharedWishHandler(
    ApplicationDbContext db,
    IReservationsApi reservationsApi,
    IUsersApi usersApi)
    : IQueryHandler<GetSharedWishQuery, SharedWishResponse>
{
    public async Task<Result<SharedWishResponse>> HandleAsync(
        GetSharedWishQuery query,
        CancellationToken ct = default)
    {
        var wish = await db.Wishes
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ShareToken == query.Token, ct);

        if (wish is null)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        var wishlist = await db.Wishlists
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == wish.WishlistId, ct);

        if (wishlist is null)
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");

        var usernameMap = await usersApi.GetUsernamesAsync([wishlist.OwnerId], ct);
        var ownerUsername = usernameMap.GetValueOrDefault(wishlist.OwnerId, "Unknown");

        var reservedIds = await reservationsApi.GetReservedWishIdsAsync([wish.Id], ct);

        var isReserved = !wishlist.IsSurpriseModeEnabled && reservedIds.Contains(wish.Id);

        return new SharedWishResponse(
            wish.Id,
            wish.WishlistId,
            wish.Name,
            wish.Description,
            wish.Price,
            wish.Currency,
            wish.Priority,
            wish.Url,
            wish.ImagePath,
            wish.IsFulfilled,
            isReserved,
            wishlist.Visibility,
            wishlist.OwnerId,
            ownerUsername);
    }
}
