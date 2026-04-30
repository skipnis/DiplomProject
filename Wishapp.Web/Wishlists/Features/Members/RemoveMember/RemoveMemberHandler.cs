using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Users;

namespace Wishapp.Web.Wishlists.Features.Members.RemoveMember;

public sealed class RemoveMemberHandler(
    ApplicationDbContext db,
    INotificationsApi notificationsApi,
    IUsersApi usersApi)
    : ICommandHandler<RemoveMemberCommand>
{
    public async Task<Result> HandleAsync(
        RemoveMemberCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (wishlist.IsSystem)
        {
            return Error.Forbidden("Wishlists.SystemWishlist", "Cannot manage members of a system wishlist");
        }

        var result = wishlist.RemoveMember(command.UserId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        var ownerUsernames = await usersApi.GetUsernamesAsync([wishlist.OwnerId], ct);
        var ownerName = ownerUsernames.GetValueOrDefault(wishlist.OwnerId);

        await notificationsApi.EnqueueAsync(command.UserId, NotificationType.RemovedFromWishlist, new
        {
            wishlistId = command.WishlistId,
            wishlistName = wishlist.Name,
            removedByUserId = wishlist.OwnerId,
            removedByDisplayName = ownerName,
        }, ct);

        return Result.Success();
    }
}
