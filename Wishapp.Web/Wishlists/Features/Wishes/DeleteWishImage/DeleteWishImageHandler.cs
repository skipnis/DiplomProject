using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.DeleteWishImage;

public sealed class DeleteWishImageHandler(
    ApplicationDbContext db,
    IStorageService storageService)
    : ICommandHandler<DeleteWishImageCommand>
{
    public async Task<Result> HandleAsync(
        DeleteWishImageCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var wish = wishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        if (wish.ImagePath is null)
        {
            return Error.NotFound("Image.NotFound", "Wish has no image");
        }

        await storageService.DeleteAsync(wish.ImagePath, ct);

        wish.RemoveImage();

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}