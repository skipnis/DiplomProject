using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Catalog.Features.GetCollections;

public sealed class GetCollectionsHandler(ApplicationDbContext db, IFusionCache cache)
    : IQueryHandler<GetCollectionsQuery, List<CatalogCollectionSummaryDto>>
{
    public async Task<Result<List<CatalogCollectionSummaryDto>>> HandleAsync(
        GetCollectionsQuery query,
        CancellationToken ct = default)
    {
        return await cache.GetOrSetAsync("catalog:collections",
            async token => await db.CatalogCollections
                .AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderBy(c => c.Order)
                .Select(c => new CatalogCollectionSummaryDto(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Occasion != null
                        ? new OccasionDto(c.Occasion.Id, c.Occasion.Key, c.Occasion.Label, c.Occasion.Order)
                        : null,
                    c.CoverImagePath,
                    c.Order,
                    c.Items.Count))
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
