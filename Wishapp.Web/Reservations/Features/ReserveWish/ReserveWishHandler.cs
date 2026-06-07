using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Reservations.Entities;
using Wishapp.Web.Wishlists;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Reservations.Features.ReserveWish;

public sealed class ReserveWishHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    IFriendshipsApi friendshipsApi,
    INotificationsApi notificationsApi)
    : ICommandHandler<ReserveWishCommand>
{
    public async Task<Result> HandleAsync(
        ReserveWishCommand command,
        CancellationToken ct = default)
    {
        var alreadyReserved = await db.WishReservations
            .AnyAsync(r => r.WishId == command.WishId, ct);

        if (alreadyReserved)
        {
            return Error.Conflict("Reservations.AlreadyReserved", "Wish is already reserved");
        }

        var accessData = await wishlistsApi.GetWishlistAccessDataAsync(command.WishlistId, ct);

        if (accessData is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (accessData.IsSystem)
        {
            return Error.Forbidden("Reservations.SystemWishlist", "Cannot reserve wishes from a system wishlist");
        }

        var wishExists = await wishlistsApi.WishExistsAsync(command.WishId, ct);

        if (!wishExists)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var isFulfilled = await wishlistsApi.IsWishFulfilledAsync(command.WishId, ct);

        if (isFulfilled)
        {
            return Error.Forbidden("Reservations.WishFulfilled", "Cannot reserve a fulfilled wish");
        }

        if (command.UserId == accessData.OwnerId)
        {
            return Error.Forbidden("Reservations.OwnWish", "Cannot reserve your own wish");
        }

        var hasAccess = await CheckAccessAsync(command.UserId, accessData, ct);

        if (!hasAccess)
        {
            return Error.Forbidden("Reservations.AccessDenied", "You do not have access to this wishlist");
        }

        var reservation = WishReservation.Create(command.WishId, command.WishlistId, command.UserId);

        db.WishReservations.Add(reservation);

        await db.SaveChangesAsync(ct);

        if (!accessData.IsSurpriseModeEnabled)
        {
            var notificationData = await wishlistsApi.GetWishNotificationDataAsync(command.WishId, ct);
            var recipientId = notificationData?.CreatedByUserId ?? accessData.OwnerId;

            await notificationsApi.EnqueueAsync(recipientId, NotificationType.WishReserved, new
            {
                wishId = command.WishId,
                wishlistId = command.WishlistId,
                wishlistName = notificationData?.WishlistName,
            }, ct);
        }

        return Result.Success();
    }

    private async Task<bool> CheckAccessAsync(
        Guid userId,
        WishlistAccessData accessData,
        CancellationToken ct)
    {
        return accessData.Visibility switch
        {
            WishlistVisibility.Public or WishlistVisibility.Friends =>
                await friendshipsApi.AreFriendsAsync(userId, accessData.OwnerId, ct),

            WishlistVisibility.SelectedFriends or WishlistVisibility.Private =>
                accessData.Members.Any(m => m.UserId == userId && m.Role >= WishlistMemberRole.Viewer),

            _ => false
        };
    }
}
