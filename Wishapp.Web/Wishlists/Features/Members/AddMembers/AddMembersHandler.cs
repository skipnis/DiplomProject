using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Members.AddMembers;

public sealed class AddMembersHandler(
    ApplicationDbContext db,
    IFriendshipsApi friendshipsApi)
    : ICommandHandler<AddMembersCommand>
{
    public async Task<Result> HandleAsync(
        AddMembersCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var userIds = command.Members.Select(m => m.UserId).ToList();

        var existingIds = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(ct);

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
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}