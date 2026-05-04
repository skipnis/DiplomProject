using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.DeclineFriend;

public sealed class DeclineFriendHandler(ApplicationDbContext db)
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

        return Result.Success();
    }
}