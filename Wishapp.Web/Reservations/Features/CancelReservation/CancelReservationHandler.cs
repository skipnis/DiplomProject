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

        var wishId = reservation.WishId;
        var wishlistId = reservation.WishlistId;

        db.WishReservations.Remove(reservation);

        await db.SaveChangesAsync(ct);

        var wishlistData = await db.Wishlists.AsNoTracking()
            .Where(wl => wl.Id == wishlistId)
            .Select(wl => new { wl.Name, wl.OwnerId, wl.IsSurpriseModeEnabled })
            .FirstOrDefaultAsync(ct);

        var wishName = await db.Wishes.AsNoTracking()
            .Where(w => w.Id == wishId)
            .Select(w => w.Name)
            .FirstOrDefaultAsync(ct);

        var cancellerName = await db.Users.AsNoTracking()
            .Where(u => u.Id == command.UserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(ct);

        if (wishlistData is not null && !wishlistData.IsSurpriseModeEnabled)
        {
            await notificationsApi.EnqueueAsync(wishlistData.OwnerId, NotificationType.ReservationCancelled, new
            {
                wishId,
                wishName,
                cancelledByUserId = command.UserId,
                cancelledByDisplayName = cancellerName,
                wishlistId,
                wishlistName = wishlistData.Name,
            }, ct);
        }

        return Result.Success();
    }
}
