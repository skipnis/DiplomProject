using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Api;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWishFromCatalog;

public sealed class AddWishFromCatalogHandler(ApplicationDbContext db, ICatalogApi catalogApi)
    : ICommandHandler<AddWishFromCatalogCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        AddWishFromCatalogCommand command,
        CancellationToken ct = default)
    {
        var catalogItem = await catalogApi.GetCatalogItemDataAsync(command.CatalogItemId, ct);

        if (catalogItem is null)
        {
            return Error.NotFound("Catalog.NotFound", "Catalog item not found");
        }

        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.AddWish(
            catalogItem.Name,
            catalogItem.Description,
            catalogItem.Price,
            catalogItem.Currency,
            Entities.WishPriority.None,
            catalogItem.Url);

        if (result.IsFailure)
        {
            return result.Error;
        }

        if (catalogItem.ImagePath is not null)
        {
            result.Value.SetImage(catalogItem.ImagePath);
        }

        db.Entry(result.Value).State = EntityState.Added;

        await db.SaveChangesAsync(ct);
        await catalogApi.IncrementWishCountAsync(command.CatalogItemId, ct);

        return result.Value.Id;
    }
}
