using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.UpdateItemDescription;

public sealed class UpdateCollectionItemDescriptionHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateCollectionItemDescriptionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateCollectionItemDescriptionCommand command,
        CancellationToken ct = default)
    {
        var collectionItem = await db.CatalogCollectionItems
            .FirstOrDefaultAsync(i => i.CollectionId == command.CollectionId && i.CatalogItemId == command.CatalogItemId, ct);

        if (collectionItem is null)
            return Error.NotFound("Catalog.CollectionItemNotFound", "Item not found in collection");

        collectionItem.UpdateDescription(command.Description);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
