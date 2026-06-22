using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Reservations.Features.CancelReservation;

public sealed class CancelReservationHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    INotificationsApi notificationsApi,
    IUsersApi usersApi)
    : ICommandHandler<CancelReservationCommand>
{
    public async Task<Result> HandleAsync(
        CancelReservationCommand command,
        CancellationToken ct = default)
    {
        var reservation = await db.WishReservations
            .FirstOrDefaultAsync(r => r.WishId == command.WishId, ct);

        if (reservation is null)
        {
            return Error.NotFound("Reservations.NotFound", "Reservation not found");
        }

        if (reservation.ReservedByUserId != command.UserId)
        {
            return Error.Forbidden("Reservations.NotOwner", "Only the person who reserved can cancel");
        }

        var isFulfilled = await wishlistsApi.IsWishFulfilledAsync(command.WishId, ct);

        if (isFulfilled)
        {
            return Error.Forbidden("Reservations.WishFulfilled", "Cannot cancel reservation for a fulfilled wish");
        }

        db.WishReservations.Remove(reservation);

        await db.SaveChangesAsync(ct);

        var notificationData = await wishlistsApi.GetWishNotificationDataAsync(command.WishId, ct);

        if (notificationData is null || notificationData.IsSurpriseModeEnabled) return Result.Success();
        
        var cancellerUsernames = await usersApi.GetUsernamesAsync([command.UserId], ct);
        var cancellerName = cancellerUsernames.GetValueOrDefault(command.UserId);

        await notificationsApi.EnqueueAsync(notificationData.OwnerId, NotificationType.ReservationCancelled, new
        {
            wishId = command.WishId,
            wishName = notificationData.WishName,
            cancelledByUserId = command.UserId,
            cancelledByDisplayName = cancellerName,
            wishlistId = notificationData.WishlistId,
            wishlistName = notificationData.WishlistName,
        }, ct);

        return Result.Success();
    }
}
