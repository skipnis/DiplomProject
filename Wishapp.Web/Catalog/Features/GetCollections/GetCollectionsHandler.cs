using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCollections;

public sealed class GetCollectionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetCollectionsQuery, List<CatalogCollectionSummaryDto>>
{
    public async Task<Result<List<CatalogCollectionSummaryDto>>> HandleAsync(
        GetCollectionsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.CatalogCollections
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .OrderBy(c => c.Order)
            .Select(c => new CatalogCollectionSummaryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Occasion,
                c.CoverImagePath,
                c.Order,
                c.Items.Count))
            .ToListAsync(ct);

        return result;
    }
}
