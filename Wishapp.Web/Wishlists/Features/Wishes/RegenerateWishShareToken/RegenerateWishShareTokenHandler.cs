using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishes.RegenerateWishShareToken;

public sealed class RegenerateWishShareTokenHandler(ApplicationDbContext db)
    : ICommandHandler<RegenerateWishShareTokenCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        RegenerateWishShareTokenCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes.Where(wish => wish.Id == command.WishId))
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");

        if (wishlist.IsSystem)
            return Error.Forbidden("Wishlists.SystemWishlist", "Cannot share wishes from a system wishlist");

        var wish = wishlist.Wishes.FirstOrDefault();

        if (wish is null)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        wish.RegenerateShareToken();

        await db.SaveChangesAsync(ct);

        return wish.ShareToken;
    }
}
