using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Users;

namespace Wishapp.Web.Friendships.Features.AcceptFriend;

public sealed class AcceptFriendHandler(
    ApplicationDbContext db,
    INotificationsApi notificationsApi,
    IUsersApi usersApi)
    : ICommandHandler<AcceptFriendCommand>
{
    public async Task<Result> HandleAsync(
        AcceptFriendCommand command,
        CancellationToken ct = default)
    {
        var friendship = await db.Friendships
            .FirstOrDefaultAsync(f =>
                f.RequesterId == command.RequesterId &&
                f.AddresseeId == command.UserId &&
                f.Status == FriendshipStatus.Pending, ct);

        if (friendship is null)
        {
            return Error.NotFound("Friendships.NotFound", "Friend request not found");
        }

        friendship.Accept();

        await db.SaveChangesAsync(ct);

        var userInfos = await usersApi.GetUsersPublicInfoAsync([command.UserId], ct);
        var accepter = userInfos.GetValueOrDefault(command.UserId);

        await notificationsApi.EnqueueAsync(command.RequesterId, NotificationType.FriendRequestAccepted, new
        {
            acceptedByUserId = command.UserId,
            acceptedByDisplayName = accepter?.DisplayName,
            acceptedByAvatarUrl = accepter?.AvatarUrl,
        }, ct);

        return Result.Success();
    }
}
