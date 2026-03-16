using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishes.UpdateWish;

public sealed class UpdateWishHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateWishCommand>
{
    public async Task<Result> HandleAsync(
        UpdateWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.UpdateWish(
            command.WishId,
            command.Name,
            command.Description,
            command.Price,
            command.Currency,
            command.Priority,
            command.Url);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}