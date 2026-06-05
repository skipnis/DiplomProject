using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.UnfulfillWish;

public sealed class UnfulfillWishHandler(ApplicationDbContext db, IGamificationApi gamificationApi)
    : ICommandHandler<UnfulfillWishCommand>
{
    public async Task<Result> HandleAsync(
        UnfulfillWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (wishlist.IsSystem)
        {
            return Error.Forbidden("Wishes.SystemWishlist", "Cannot unfulfill wishes from a system wishlist");
        }

        if (await gamificationApi.HasGiftBadgesAsync(command.WishId, ct))
        {
            return Error.Conflict("Wishes.GiftBadges.AlreadyRated", "Cannot unfulfill a wish that has already been rated");
        }

        var result = wishlist.UnfulfillWish(command.WishId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.FulfilledWishRecords
            .Where(r => r.WishId == command.WishId)
            .ExecuteDeleteAsync(ct);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
