using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.AcceptFriend;

public sealed class AcceptFriendHandler(ApplicationDbContext db)
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

        friendship.Status = FriendshipStatus.Accepted;
        
        friendship.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}