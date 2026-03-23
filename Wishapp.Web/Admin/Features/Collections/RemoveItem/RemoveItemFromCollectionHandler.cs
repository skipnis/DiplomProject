using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.RemoveItem;

public sealed class RemoveItemFromCollectionHandler(ApplicationDbContext db)
    : ICommandHandler<RemoveItemFromCollectionCommand>
{
    public async Task<Result> HandleAsync(
        RemoveItemFromCollectionCommand command,
        CancellationToken ct = default)
    {
        var item = await db.CatalogCollectionItems
            .FirstOrDefaultAsync(
                i => i.CollectionId == command.CollectionId && i.CatalogItemId == command.CatalogItemId,
                ct);

        if (item is null)
            return Error.NotFound("Catalog.ItemNotInCollection", "Item not found in collection");

        db.CatalogCollectionItems.Remove(item);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
