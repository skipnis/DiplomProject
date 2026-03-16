using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DeleteWishlist;

public sealed class DeleteWishlistHandler(ApplicationDbContext db)
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

        db.Wishlists.Remove(wishlist);
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}