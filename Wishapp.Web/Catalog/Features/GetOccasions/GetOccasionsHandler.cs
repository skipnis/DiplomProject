using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Catalog.Features.GetOccasions;

public sealed class GetOccasionsHandler(ApplicationDbContext db, IFusionCache cache)
    : IQueryHandler<GetOccasionsQuery, List<OccasionDto>>
{
    public async Task<Result<List<OccasionDto>>> HandleAsync(
        GetOccasionsQuery query,
        CancellationToken ct = default)
    {
        return await cache.GetOrSetAsync("catalog:occasions",
            async token => await db.CatalogOccasions
                .AsNoTracking()
                .OrderBy(o => o.Order)
                .Select(o => new OccasionDto(o.Id, o.Key, o.Label, o.Order))
                .ToListAsync(token),
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(1),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromDays(1),
            },
            ct);
    }
}
