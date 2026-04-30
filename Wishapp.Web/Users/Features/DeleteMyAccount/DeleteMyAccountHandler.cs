using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Reservations;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.DeleteMyAccount;

public sealed class DeleteMyAccountHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    IReservationsApi reservationsApi,
    IFriendshipsApi friendshipsApi,
    IGamificationApi gamificationApi,
    IStorageService storageService)
    : ICommandHandler<DeleteMyAccountCommand>
{
    public async Task<Result> HandleAsync(DeleteMyAccountCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", "User not found");

        if (user.AvatarPath is not null)
        {
            await storageService.DeleteAsync(user.AvatarPath, ct);
        }

        await reservationsApi.DeleteUserDataAsync(command.UserId, ct);
        await gamificationApi.DeleteUserDataAsync(command.UserId, ct);
        await wishlistsApi.DeleteUserDataAsync(command.UserId, ct);
        await friendshipsApi.DeleteUserDataAsync(command.UserId, ct);

        await db.Events
            .Where(e => e.OwnerId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.UserExternalTokens
            .Where(t => t.UserId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.RefreshTokens
            .Where(t => t.UserId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.AuthIdentities
            .Where(a => a.UserId == command.UserId)
            .ExecuteDeleteAsync(ct);

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
