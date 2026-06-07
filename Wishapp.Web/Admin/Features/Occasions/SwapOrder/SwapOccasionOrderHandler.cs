using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Occasions.SwapOrder;

public sealed class SwapOccasionOrderHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<SwapOccasionOrderCommand>
{
    public async Task<Result> HandleAsync(SwapOccasionOrderCommand command, CancellationToken ct = default)
    {
        var occasion = await db.CatalogOccasions.FirstOrDefaultAsync(o => o.Id == command.Id, ct);
        if (occasion is null)
            return Error.NotFound("Catalog.OccasionNotFound", "Occasion not found");

        var target = await db.CatalogOccasions.FirstOrDefaultAsync(o => o.Id == command.TargetId, ct);
        if (target is null)
            return Error.NotFound("Catalog.OccasionNotFound", "Target occasion not found");

        var tempOrder = occasion.Order;
        occasion.SetOrder(target.Order);
        target.SetOrder(tempOrder);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:occasions", token: ct);

        return Result.Success();
    }
}
