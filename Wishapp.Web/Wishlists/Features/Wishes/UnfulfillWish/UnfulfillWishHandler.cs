using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.UnfulfillWish;

public sealed class UnfulfillWishHandler(ApplicationDbContext db)
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

        if (wishlist.SystemType == SystemWishlistType.Blacklist)
        {
            return Error.Forbidden("Wishes.BlacklistWishlist", "Cannot unfulfill wishes from a blacklist wishlist");
        }

        var result = wishlist.UnfulfillWish(command.WishId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
