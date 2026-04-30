using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Reservations;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.FulfillWish;

public sealed class FulfillWishHandler(
    ApplicationDbContext db,
    INotificationsApi notificationsApi,
    IReservationsApi reservationsApi,
    IUsersApi usersApi)
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

        if (wishlist.IsSystem)
        {
            return Error.Forbidden("Wishes.SystemWishlist", "Cannot fulfill wishes from a system wishlist");
        }

        var reserverId = await reservationsApi.GetReserverForWishAsync(command.WishId, ct);

        var result = wishlist.FulfillWish(command.WishId, command.UserId, reserverId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        await reservationsApi.DeleteReservationForWishAsync(command.WishId, ct);

        if (reserverId.HasValue)
        {
            var wish = wishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);

            var ownerUsernames = await usersApi.GetUsernamesAsync([wishlist.OwnerId], ct);
            var ownerName = ownerUsernames.GetValueOrDefault(wishlist.OwnerId);

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
