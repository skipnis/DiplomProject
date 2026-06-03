using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Users;

namespace Wishapp.Web.Friendships.Features.DeclineFriend;

public sealed class DeclineFriendHandler(
    ApplicationDbContext db,
    INotificationsApi notificationsApi,
    IUsersApi usersApi)
    : ICommandHandler<DeclineFriendCommand>
{
    public async Task<Result> HandleAsync(
        DeclineFriendCommand command,
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

        friendship.Decline();

        await db.SaveChangesAsync(ct);

        var userInfos = await usersApi.GetUsersPublicInfoAsync([command.UserId], ct);
        var decliner = userInfos.GetValueOrDefault(command.UserId);

        await notificationsApi.EnqueueAsync(command.RequesterId, NotificationType.FriendRequestDeclined, new
        {
            declinedByUserId = command.UserId,
            declinedByDisplayName = decliner?.DisplayName,
            declinedByAvatarUrl = decliner?.AvatarUrl,
        }, ct);

        return Result.Success();
    }
}