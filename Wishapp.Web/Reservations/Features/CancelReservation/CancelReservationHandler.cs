using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Reservations.Features.CancelReservation;

public sealed class CancelReservationHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    INotificationsApi notificationsApi)
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

        if (notificationData is not null && !notificationData.IsSurpriseModeEnabled)
        {
            var recipientId = notificationData.CreatedByUserId ?? notificationData.OwnerId;

            await notificationsApi.EnqueueAsync(recipientId, NotificationType.ReservationCancelled, new
            {
                wishId = command.WishId,
                wishlistId = notificationData.WishlistId,
                wishlistName = notificationData.WishlistName,
            }, ct);
        }

        return Result.Success();
    }
}
