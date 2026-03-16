using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWish;

public record AddWishResponse(Guid WishId);

public sealed class AddWishHandler(ApplicationDbContext db)
    : ICommandHandler<AddWishCommand, AddWishResponse>
{
    public async Task<Result<AddWishResponse>> HandleAsync(
        AddWishCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }
        
        var result = wishlist.AddWish(
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
        
        db.Entry(result.Value).State = EntityState.Added;

        await db.SaveChangesAsync(ct);

        return new AddWishResponse(result.Value.Id);
    }
}