using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Collections.Create;

public sealed class CreateCollectionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<CreateCollectionCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCollectionCommand command,
        CancellationToken ct = default)
    {
        var nextOrder = await db.CatalogCollections.AnyAsync(ct)
            ? await db.CatalogCollections.MaxAsync(c => c.Order, ct) + 1
            : 1;

        var collection = CatalogCollection.Create(
            command.Name,
            command.Description,
            command.OccasionId,
            command.CoverImagePath,
            nextOrder);

        db.CatalogCollections.Add(collection);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:collections", token: ct);

        return collection.Id;
    }
}
