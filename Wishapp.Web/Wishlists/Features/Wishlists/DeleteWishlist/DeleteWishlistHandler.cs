using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DeleteWishlist;

public sealed class DeleteWishlistHandler(ApplicationDbContext db, IGamificationApi gamificationApi)
    : ICommandHandler<DeleteWishlistCommand>
{
    public async Task<Result> HandleAsync(
        DeleteWishlistCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.Delete();

        if (result.IsFailure)
        {
            return result.Error;
        }

        var wishIds = await db.Wishes
            .Where(w => w.WishlistId == command.WishlistId)
            .Select(w => w.Id)
            .ToListAsync(ct);

        await gamificationApi.DeleteBadgesForWishesAsync(wishIds, ct);

        db.Wishlists.Remove(wishlist);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
