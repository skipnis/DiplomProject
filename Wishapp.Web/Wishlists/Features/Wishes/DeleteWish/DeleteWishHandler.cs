using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.DeleteWish;

public sealed class DeleteWishHandler(ApplicationDbContext db, IStorageService storageService)
    : ICommandHandler<DeleteWishCommand>
{
    public async Task<Result> HandleAsync(
        DeleteWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");

        var wish = wishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);

        if (wish is null)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        // Удаляем картинку из MinIO если есть
        if (wish.ImagePath is not null)
            await storageService.DeleteAsync(wish.ImagePath, ct);

        var result = wishlist.RemoveWish(command.WishId);

        if (result.IsFailure)
            return result.Error;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}