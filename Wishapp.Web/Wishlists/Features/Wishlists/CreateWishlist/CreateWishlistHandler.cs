using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;

public sealed class CreateWishlistHandler(
    ApplicationDbContext db,
    IFriendshipsApi friendshipsApi)
    : ICommandHandler<CreateWishlistCommand, CreateWishlistResponse>
{
    public async Task<Result<CreateWishlistResponse>> HandleAsync(
        CreateWishlistCommand command,
        CancellationToken ct = default)
    {
        var wishlist = Wishlist.Create(
            command.OwnerId,
            command.Name,
            command.Description,
            command.Emoji,
            command.Visibility);

        if (command.Members is { Count: > 0 })
        {
            var membersIds = command.Members.Select(m => m.UserId).ToList();

            var existingUsersIds = await db.Users
                .Where(u => membersIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync(ct);

            var missingId = membersIds.FirstOrDefault(id => !existingUsersIds.Contains(id));
            
            if (missingId != Guid.Empty)
            {
                return Error.NotFound("Users.NotFound", $"User {missingId} not found");
            }

            var friendIds = await friendshipsApi.GetFriendIdsAsync(command.OwnerId, membersIds, ct);

            var notFriendId = membersIds.FirstOrDefault(id => !friendIds.Contains(id));
            
            if (notFriendId != Guid.Empty)
            {
                return Error.Failure("Friendships.NotFriend", $"User {notFriendId} is not your friend");
            }

            foreach (var invitedMember in command.Members)
            {
                var result = wishlist.AddMember(invitedMember.UserId, invitedMember.Role);
                
                if (result.IsFailure) return result.Error;
            }
        }

        db.Wishlists.Add(wishlist);
        
        await db.SaveChangesAsync(ct);

        return new CreateWishlistResponse(
            wishlist.Id,
            wishlist.Name,
            wishlist.Description,
            wishlist.Emoji,
            wishlist.Visibility,
            wishlist.CreatedAt);
    }
}