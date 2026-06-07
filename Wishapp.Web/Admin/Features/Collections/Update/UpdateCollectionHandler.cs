using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Collections.Update;

public sealed class UpdateCollectionHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<UpdateCollectionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateCollectionCommand command,
        CancellationToken ct = default)
    {
        var collection = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        collection.Update(
            command.Name,
            command.Description,
            command.OccasionId,
            command.CoverImagePath,
            command.IsPublished);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:collections", token: ct);

        return Result.Success();
    }
}
