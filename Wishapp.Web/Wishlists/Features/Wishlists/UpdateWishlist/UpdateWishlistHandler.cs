using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishlists.UpdateWishlist;

public sealed class UpdateWishlistHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateWishlistCommand>
{
    public async Task<Result> HandleAsync(
        UpdateWishlistCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.Update(
            command.Name,
            command.Description,
            command.Emoji,
            command.Visibility);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}