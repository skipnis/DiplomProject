using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Users;

namespace Wishapp.Web.Wishlists.Features.Members.AddMembers;

public sealed class AddMembersHandler(
    ApplicationDbContext db,
    IFriendshipsApi friendshipsApi,
    INotificationsApi notificationsApi,
    IUsersApi usersApi)
    : ICommandHandler<AddMembersCommand>
{
    public async Task<Result> HandleAsync(
        AddMembersCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
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

        var userIds = command.Members.Select(m => m.UserId).ToList();

        var existingIds = await usersApi.FilterExistingIdsAsync(userIds, ct);

        var missingId = userIds.FirstOrDefault(id => !existingIds.Contains(id));

        if (missingId != Guid.Empty)
        {
            return Error.NotFound("Users.NotFound", $"User {missingId} not found");
        }

        var friendIds = await friendshipsApi.GetFriendIdsAsync(command.OwnerId, userIds, ct);

        var notFriendId = userIds.FirstOrDefault(id => !friendIds.Contains(id));

        if (notFriendId != Guid.Empty)
        {
            return Error.Failure("Friendships.NotFriend", $"User {notFriendId} is not your friend");
        }

        foreach (var invite in command.Members)
        {
            var result = wishlist.AddMember(invite.UserId, invite.Role);

            if (result.IsFailure)
            {
                return result.Error;
            }

            db.WishlistMembers.Add(result.Value);
        }

        await db.SaveChangesAsync(ct);

        var ownerUsernames = await usersApi.GetUsernamesAsync([command.OwnerId], ct);
        var ownerName = ownerUsernames.GetValueOrDefault(command.OwnerId);

        foreach (var invite in command.Members)
        {
            await notificationsApi.EnqueueAsync(invite.UserId, NotificationType.AddedToWishlist, new
            {
                wishlistId = command.WishlistId,
                wishlistName = wishlist.Name,
                addedByUserId = command.OwnerId,
                addedByDisplayName = ownerName,
                role = (int)invite.Role,
            }, ct);
        }

        return Result.Success();
    }
}
