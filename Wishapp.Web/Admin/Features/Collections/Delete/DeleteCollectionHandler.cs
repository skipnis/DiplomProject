using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Collections.Delete;

public sealed class DeleteCollectionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<DeleteCollectionCommand>
{
    public async Task<Result> HandleAsync(
        DeleteCollectionCommand command,
        CancellationToken ct = default)
    {
        var collection = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        db.CatalogCollections.Remove(collection);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:collections", token: ct);

        return Result.Success();
    }
}
