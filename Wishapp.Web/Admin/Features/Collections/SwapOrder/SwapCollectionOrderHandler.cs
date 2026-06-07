using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Collections.SwapOrder;

public sealed class SwapCollectionOrderHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<SwapCollectionOrderCommand>
{
    public async Task<Result> HandleAsync(SwapCollectionOrderCommand command, CancellationToken ct = default)
    {
        var collection = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.Id, ct);
        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        var target = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.TargetId, ct);
        if (target is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Target collection not found");

        var tempOrder = collection.Order;
        collection.SetOrder(target.Order);
        target.SetOrder(tempOrder);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:collections", token: ct);

        return Result.Success();
    }
}
