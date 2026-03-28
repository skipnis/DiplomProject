using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DuplicateWish;

public sealed class DuplicateWishHandler(ApplicationDbContext db, IStorageService storage)
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

        var original = wishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);

        var result = wishlist.DuplicateWish(command.WishId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        if (original?.ImagePath is not null)
        {
            var newImagePath = StoragePaths.WishImage(command.WishlistId, result.Value.Id);
            await storage.CopyAsync(original.ImagePath, newImagePath, ct);
            result.Value.SetImage(newImagePath);
        }

        db.Entry(result.Value).State = EntityState.Added;

        await db.SaveChangesAsync(ct);

        return new DuplicateWishResponse(result.Value.Id);
    }
}