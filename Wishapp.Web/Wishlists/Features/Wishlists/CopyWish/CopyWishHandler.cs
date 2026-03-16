using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CopyWish;

public sealed class CopyWishHandler(ApplicationDbContext db)
    : ICommandHandler<CopyWishCommand, CopyWishResponse>
{
    public async Task<Result<CopyWishResponse>> HandleAsync(
        CopyWishCommand command,
        CancellationToken ct = default)
    {
        var wishlists = await db.Wishlists
            .Include(w => w.Wishes)
            .Where(w => w.Id == command.SourceWishlistId || w.Id == command.TargetWishlistId)
            .ToListAsync(ct);

        var sourceWishlist = wishlists.FirstOrDefault(w => w.Id == command.SourceWishlistId);
        
        var targetWishlist = wishlists.FirstOrDefault(w => w.Id == command.TargetWishlistId);
        
        if (sourceWishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Source wishlist not found");
        }

        if (targetWishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Target wishlist not found");
        }

        var wish = sourceWishlist.Wishes.FirstOrDefault(w => w.Id == command.WishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var result = targetWishlist.CopyWishFrom(wish);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return new CopyWishResponse(result.Value.Id);
    }
}