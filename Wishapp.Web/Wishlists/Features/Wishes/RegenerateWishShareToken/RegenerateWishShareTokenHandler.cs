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
        var wish = await db.Wishes
            .FirstOrDefaultAsync(w => w.Id == command.WishId && w.WishlistId == command.WishlistId, ct);

        if (wish is null)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        wish.RegenerateShareToken();

        await db.SaveChangesAsync(ct);

        return wish.ShareToken;
    }
}
