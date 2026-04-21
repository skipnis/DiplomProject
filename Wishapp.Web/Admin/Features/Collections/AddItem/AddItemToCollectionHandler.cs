using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.AddItem;

public sealed class AddItemToCollectionHandler(ApplicationDbContext db)
    : ICommandHandler<AddItemToCollectionCommand>
{
    public async Task<Result> HandleAsync(
        AddItemToCollectionCommand command,
        CancellationToken ct = default)
    {
        var collectionExists = await db.CatalogCollections
            .AnyAsync(c => c.Id == command.CollectionId, ct);

        if (!collectionExists)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        var itemExists = await db.CatalogItems
            .AnyAsync(i => i.Id == command.CatalogItemId, ct);

        if (!itemExists)
            return Error.NotFound("Catalog.ItemNotFound", "Catalog item not found");

        var alreadyAdded = await db.CatalogCollectionItems
            .AnyAsync(i => i.CollectionId == command.CollectionId && i.CatalogItemId == command.CatalogItemId, ct);

        if (alreadyAdded)
            return Error.Conflict("Catalog.ItemAlreadyInCollection", "Item already in collection");

        var collectionItem = CatalogCollectionItem.Create(command.CollectionId, command.CatalogItemId, command.Description);
        db.CatalogCollectionItems.Add(collectionItem);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
