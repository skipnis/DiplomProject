using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.FulfillWish;

public sealed class FulfillWishHandler(ApplicationDbContext db, INotificationsApi notificationsApi)
    : ICommandHandler<FulfillWishCommand>
{
    public async Task<Result> HandleAsync(
        FulfillWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (wishlist.SystemType == SystemWishlistType.Blacklist)
        {
            return Error.Forbidden("Wishes.BlacklistWishlist", "Cannot fulfill wishes from a blacklist wishlist");
        }

        var reserverId = await db.WishReservations
            .Where(r => r.WishId == command.WishId)
            .Select(r => (Guid?)r.ReservedByUserId)
            .FirstOrDefaultAsync(ct);

        var result = wishlist.FulfillWish(command.WishId, command.UserId, reserverId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        await db.WishReservations
            .Where(r => r.WishId == command.WishId)
            .ExecuteDeleteAsync(ct);

        if (reserverId.HasValue)
        {
            var wish = wishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);
            var ownerName = await db.Users.AsNoTracking()
                .Where(u => u.Id == wishlist.OwnerId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(ct);

            await notificationsApi.EnqueueAsync(reserverId.Value, NotificationType.WishFulfilled, new
            {
                wishId = command.WishId,
                wishName = wish?.Name,
                wishlistOwnerId = wishlist.OwnerId,
                wishlistOwnerDisplayName = ownerName,
                wishlistId = command.WishlistId,
                wishlistName = wishlist.Name,
            }, ct);
        }

        return Result.Success();
    }
}
