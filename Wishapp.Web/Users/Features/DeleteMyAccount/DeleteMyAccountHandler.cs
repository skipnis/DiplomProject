using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.DeleteMyAccount;

public sealed class DeleteMyAccountHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteMyAccountCommand>
{
    public async Task<Result> HandleAsync(DeleteMyAccountCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", "User not found");

        var ownedWishlistIds = await db.Wishlists
            .Where(w => w.OwnerId == command.UserId)
            .Select(w => w.Id)
            .ToListAsync(ct);

        await db.WishReservations
            .Where(r => ownedWishlistIds.Contains(r.WishlistId) || r.ReservedByUserId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.Wishlists
            .Where(w => w.OwnerId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.WishlistMembers
            .Where(m => m.UserId == command.UserId)
            .ExecuteDeleteAsync(ct);

        await db.Friendships
            .Where(f => f.RequesterId == command.UserId || f.AddresseeId == command.UserId)
            .ExecuteDeleteAsync(ct);

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
