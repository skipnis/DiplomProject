using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DuplicateWish;

public sealed class DuplicateWishHandler(ApplicationDbContext db)
    : ICommandHandler<DuplicateWishCommand, DuplicateWishResponse>
{
    public async Task<Result<DuplicateWishResponse>> HandleAsync(
        DuplicateWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.DuplicateWish(command.WishId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return new DuplicateWishResponse(result.Value.Id);
    }
}