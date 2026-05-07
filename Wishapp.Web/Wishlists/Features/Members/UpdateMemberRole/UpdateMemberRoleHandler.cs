using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Members.UpdateMemberRole;

public record UpdateMemberRoleCommand(
    Guid WishlistId,
    Guid UserId,
    WishlistMemberRole Role,
    string? CustomRoleName) : ICommand;

public sealed class UpdateMemberRoleHandler(ApplicationDbContext db, INotificationsApi notificationsApi)
    : ICommandHandler<UpdateMemberRoleCommand>
{
    public async Task<Result> HandleAsync(
        UpdateMemberRoleCommand command,
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

        var result = wishlist.UpdateMemberRole(command.UserId, command.Role, command.CustomRoleName);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        var updatedMember = wishlist.Members.First(m => m.UserId == command.UserId);
        if (updatedMember.Role != WishlistMemberRole.Owner)
        {
            await notificationsApi.EnqueueAsync(command.UserId, NotificationType.WishlistRoleUpdated, new
            {
                wishlistId = command.WishlistId,
                wishlistName = wishlist.Name,
                newRole = (int)command.Role,
                customRoleName = command.CustomRoleName,
            }, ct);
        }

        return Result.Success();
    }
}
