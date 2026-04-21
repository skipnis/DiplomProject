using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Catalog.Features.GetCategories;

public sealed class GetCategoriesHandler(ApplicationDbContext db, IFusionCache cache)
    : IQueryHandler<GetCategoriesQuery, List<CatalogCategoryDto>>
{
    public async Task<Result<List<CatalogCategoryDto>>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken ct = default)
    {
        return await cache.GetOrSetAsync("catalog:categories",
            async token => await db.CatalogCategories
                .AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderBy(c => c.Order)
                .Select(c => new CatalogCategoryDto(c.Id, c.Name, c.Order, c.IsPublished))
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
