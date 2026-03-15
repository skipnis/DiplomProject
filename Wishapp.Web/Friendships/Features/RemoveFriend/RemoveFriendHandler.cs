using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.RemoveFriend;

public sealed class RemoveFriendHandler(ApplicationDbContext db)
    : ICommandHandler<RemoveFriendCommand>
{
    public async Task<Result> HandleAsync(
        RemoveFriendCommand command,
        CancellationToken ct = default)
    {
        var friendship = await db.Friendships
            .FirstOrDefaultAsync(Friendship.Between(command.UserId, command.FriendId), ct);

        if (friendship is null)
        {
            return Error.NotFound("Friendships.NotFound", "Friendship not found");
        }

        db.Friendships.Remove(friendship);
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}