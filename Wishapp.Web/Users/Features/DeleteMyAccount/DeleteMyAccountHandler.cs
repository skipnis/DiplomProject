using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Proposals;
using Wishapp.Web.Reservations;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.DeleteMyAccount;

public sealed class DeleteMyAccountHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    IReservationsApi reservationsApi,
    IFriendshipsApi friendshipsApi,
    IGamificationApi gamificationApi,
    IProposalsApi proposalsApi,
    IStorageService storageService)
    : ICommandHandler<DeleteMyAccountCommand>
{
    private static readonly Error InvalidCode =
        Error.Unauthorized("Otp.InvalidCode", "Invalid or expired code.");

    public async Task<Result> HandleAsync(DeleteMyAccountCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", "User not found");

        var email = user.Email.Trim().ToLowerInvariant();
        var codeHash = HashCode(command.Code);

        var otp = await db.EmailOtps
            .Where(o => o.Email == email && !o.UsedAt.HasValue)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null || otp.IsExpired)
            return InvalidCode;

        if (otp.CodeHash != codeHash)
        {
            otp.IncrementAttempt();
            await db.SaveChangesAsync(ct);
            return InvalidCode;
        }

        if (!otp.IsValid)
            return InvalidCode;

        otp.MarkUsed();
        await db.SaveChangesAsync(ct);

        if (user.AvatarPath is not null)
            await storageService.DeleteAsync(user.AvatarPath, ct);

        await reservationsApi.DeleteUserDataAsync(command.UserId, ct);
        await proposalsApi.DeleteUserDataAsync(command.UserId, ct);
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

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }
}
