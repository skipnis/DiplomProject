using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users;

namespace Wishapp.Web.Friendships.Features.MakeFriend;

public sealed class MakeFriendHandler(
    ApplicationDbContext db,
    IUsersApi usersApi)
    : ICommandHandler<MakeFriendCommand>
{
    public async Task<Result> HandleAsync(
        MakeFriendCommand command,
        CancellationToken ct = default)
    {
        if (command.RequesterId == command.AddresseeId)
        {
            return Error.Failure("Friendships.SelfRequest", "Cannot send friend request to yourself");
        }

        var addresseeExists = await usersApi.ExistsAsync(command.AddresseeId, ct);

        if (addresseeExists.IsFailure)
        {
            return Result.Failure(addresseeExists.Error);
        }

        var existingFriendship = await db.Friendships
            .FirstOrDefaultAsync(Friendship.Between(command.RequesterId, command.AddresseeId), ct);

        if (existingFriendship is not null)
        {
            return Error.Conflict("Friendships.AlreadyExists", "Friendship already exists");
        }

        var friendship = Friendship.Create(command.RequesterId, command.AddresseeId);

        db.Friendships.Add(friendship);
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}