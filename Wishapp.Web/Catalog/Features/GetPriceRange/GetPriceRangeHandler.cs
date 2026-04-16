using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Catalog.Features.GetPriceRange;

public sealed class GetPriceRangeHandler(ApplicationDbContext db, IFusionCache cache)
    : IQueryHandler<GetPriceRangeQuery, PriceRangeResult>
{
    public async Task<Result<PriceRangeResult>> HandleAsync(
        GetPriceRangeQuery query,
        CancellationToken ct = default)
    {
        var max = await cache.GetOrSetAsync("catalog:price-range",
            async token =>
            {
                var value = await db.CatalogItems
                    .AsNoTracking()
                    .Where(i => i.IsPublished && i.Price != null)
                    .MaxAsync(i => i.Price, token) ?? 0;
                return (int)Math.Ceiling(value);
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(1),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromDays(1),
            },
            ct);

        return new PriceRangeResult(max);
    }
}
