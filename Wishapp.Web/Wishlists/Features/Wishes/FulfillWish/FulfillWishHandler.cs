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
    : ICommandHandler<FulfillWishCommand, FulfillWishResult>
{
    public async Task<Result<FulfillWishResult>> HandleAsync(
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

        var reserverId = await reservationsApi.GetReserverForWishAsync(command.WishId, ct);

        var result = wishlist.FulfillWish(command.WishId, command.UserId, reserverId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        var fulfilledWish = wishlist.Wishes.First(w => w.Id == command.WishId);

        var wishOwnerId = fulfilledWish.CreatedByUserId ?? wishlist.OwnerId;

        var record = FulfilledWishRecord.Create(
            command.WishId,
            wishOwnerId,
            reserverId,
            fulfilledWish.Name,
            fulfilledWish.Description,
            fulfilledWish.Price,
            fulfilledWish.Currency,
            fulfilledWish.ImagePath,
            wishlist.Name,
            wishlist.SystemType == SystemWishlistType.Hidden || wishlist.IsSurpriseModeEnabled,
            fulfilledWish.FulfilledAt!.Value);

        db.FulfilledWishRecords.Add(record);

        await db.SaveChangesAsync(ct);

        await reservationsApi.DeleteReservationForWishAsync(command.WishId, ct);

        if (reserverId.HasValue)
        {
            var ownerUsernames = await usersApi.GetUsernamesAsync([wishlist.OwnerId], ct);
            var ownerName = ownerUsernames.GetValueOrDefault(wishlist.OwnerId);

            await notificationsApi.EnqueueAsync(reserverId.Value, NotificationType.WishFulfilled, new
            {
                wishId = command.WishId,
                wishName = fulfilledWish.Name,
                wishlistOwnerId = wishlist.OwnerId,
                wishlistOwnerDisplayName = ownerName,
                wishlistId = command.WishlistId,
                wishlistName = wishlist.Name,
            }, ct);
        }

        return new FulfillWishResult(reserverId.HasValue);
    }
}
