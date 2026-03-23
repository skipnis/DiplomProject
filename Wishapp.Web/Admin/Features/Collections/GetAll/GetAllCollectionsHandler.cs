using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.GetAll;

public sealed class GetAllCollectionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllCollectionsQuery, List<CatalogCollectionAdminDto>>
{
    public async Task<Result<List<CatalogCollectionAdminDto>>> HandleAsync(
        GetAllCollectionsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.CatalogCollections
            .AsNoTracking()
            .OrderBy(c => c.Order)
            .Select(c => new CatalogCollectionAdminDto(
                c.Id,
                c.Name,
                c.Description,
                c.Occasion,
                c.CoverImagePath,
                c.Order,
                c.IsPublished,
                c.Items.Count,
                c.CreatedAt))
            .ToListAsync(ct);

        return result;
    }
}
